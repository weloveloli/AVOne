// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Tool
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Internal;
    using System.Reflection;
    using System.Threading.Tasks;
    using AVOne.Abstraction;
    using AVOne.Common.Migrations;
    using AVOne.Common.Plugins;
    using AVOne.Configuration;
    using AVOne.Extensions;
    using AVOne.Impl.Extensions;
    using AVOne.Impl.IO;
    using AVOne.Impl.Plugins;
    using AVOne.Impl.Registrator;
    using AVOne.IO;
    using AVOne.Providers.Jellyfin;
    using AVOne.Providers.Official.Metadata;
    using AVOne.Tool.Commands;
    using AVOne.Tool.Configuration;
    using AVOne.Tool.Facade;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using My.Extensions.Localization.Json;

    internal class ConsoleAppHost : IApplicationHost, IAsyncDisposable, IDisposable
    {
        private readonly BaseHostOptions _option;
        private readonly CancellationTokenSource _tokenSource;
        private bool _disposed = false;
        private List<Type> _creatingInstances;
        /// <summary>
        /// The disposable parts.
        /// </summary>
        private readonly ConcurrentDictionary<IDisposable, byte> _disposableParts = new();

        private readonly IXmlSerializer _xmlSerializer;

        private readonly IFileSystem _fileSystem;

        private readonly IPluginManager _pluginManager;

        public Version ApplicationVersion { get; }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        public ConsoleApplicationPaths AppPaths { get; }

        public ConsoleConfigurationManager ConfigurationManager { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger<ConsoleAppHost> Logger { get; }

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets or sets all concrete types.
        /// </summary>
        /// <value>All concrete types.</value>
        private Type[] _allConcreteTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleAppHost"/> class.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="tokenSource">The token source.</param>
        /// <param name="path">The path.</param>
        public ConsoleAppHost(BaseHostOptions option, ILoggerFactory loggerFactory, CancellationTokenSource tokenSource, ConsoleApplicationPaths path)
        {
            _option = option;
            LoggerFactory = loggerFactory;
            _tokenSource = tokenSource;

            // Get the version from the assembly
            var assembly = GetExecutingOrEntryAssembly();
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            ApplicationVersion = attribute is not null ? Version.Parse(attribute.InformationalVersion.Substring(0, attribute.InformationalVersion.IndexOf('+'))) : assembly.GetName().Version;
            AppPaths = path;
            Logger = loggerFactory.CreateLogger<ConsoleAppHost>();
            _fileSystem = new ManagedFileSystem(LoggerFactory.CreateLogger<ManagedFileSystem>(), path);
            _xmlSerializer = new DefaultXmlSerializer();
            ConfigurationManager = new ConsoleConfigurationManager(path, loggerFactory, _xmlSerializer, _fileSystem);
            _pluginManager = new PluginManager(LoggerFactory.CreateLogger<PluginManager>(), this, ConfigurationManager.Configuration, path.PluginsPath, ApplicationVersion);
        }

        private static Assembly GetExecutingOrEntryAssembly()
        {
            //resolve issues of null EntryAssembly in Xunit Test #392,424,389
            //return Assembly.GetEntryAssembly();
            return Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
        }

        /// <summary>
        /// Discovers the types.
        /// </summary>
        protected void DiscoverTypes()
        {
            Logger.LogInformation("Loading assemblies");
            _allConcreteTypes = GetTypes(GetComposablePartAssemblies()).ToArray();
        }

        public void Init(IServiceCollection services)
        {
            DiscoverTypes();
            services.TryAdd(ServiceDescriptor.Singleton(LoggerFactory));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
            _ = services.AddHttpClient();
#if RELEASE
            var localizationSettings = App.GetConfig<LocalizationSettingsOptions>("LocalizationSettings", true);
            localizationSettings.AssemblyName = GetExecutingOrEntryAssembly().GetName().Name;
            Console.WriteLine(localizationSettings.AssemblyName);
            services.AddConfigurableOptions<LocalizationSettingsOptions>();

            services.TryAddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.Configure<JsonLocalizationOptions>((options =>
            {
                if (!string.IsNullOrWhiteSpace(localizationSettings.ResourcesPath))
                    options.ResourcesPath = localizationSettings.ResourcesPath;
            }));
#else
            services.AddDefaultHostLocalization();
#endif
            RegisterServices(services);
            var registrators = GetExports<IServiceRegistrator>().ToList();
            registrators.ForEach(e => e.RegisterServices(services));
            _pluginManager.RegisterServices(services);
        }

        public void PostBuildService()
        {
            _pluginManager.CreatePlugins();
            var registrators = GetExports<IServiceRegistrator>().ToList();
            registrators.ForEach(e => e.PostBuildService(this));
        }

        /// <summary>
        /// Registers services/resources with the service collection that will be available via DI.
        /// </summary>
        /// <param name="service">Instance of the <see cref="IServiceCollection"/> interface.</param>
        protected void RegisterServices(IServiceCollection service)
        {
            _option.InitService(service);
            service.AddMemoryCache();
            service.AddSingleton<IApplicationHost>(this);
            service.AddSingleton<IStartupOptions>(_option);
            service.AddSingleton<IApplicationPaths>(AppPaths);
            service.AddSingleton(_xmlSerializer);
            service.AddSingleton(_fileSystem);
            service.AddSingleton<IConfigurationManager>(ConfigurationManager);
            service.AddSingleton<IMetaDataFacade, MetaDataFacade>();
            service.AddSingleton(_pluginManager);
        }

        /// <summary>
        /// Gets the composable part assemblies.
        /// </summary>
        /// <returns>IEnumerable{Assembly}.</returns>
        protected IEnumerable<Assembly> GetComposablePartAssemblies()
        {
            foreach (var p in _pluginManager.LoadAssemblies())
            {
                yield return p;
            }

            // Include composable parts in the AVOne.Impl assembly
            yield return typeof(ImplRegistrator).Assembly;
            yield return typeof(MigrationsFactory).Assembly;
            yield return typeof(JellyfinNamingOptionProvider).Assembly;
            yield return typeof(ImageSaverProvider).Assembly;
        }

        private IEnumerable<Type> GetTypes(IEnumerable<Assembly> assemblies)
        {
            foreach (var ass in assemblies)
            {
                Type[] exportedTypes;
                try
                {
                    exportedTypes = ass.GetExportedTypes();
                }
                catch (FileNotFoundException ex)
                {
                    Logger.LogError(ex, "Error getting exported types from {Assembly}", ass.FullName);
                    _pluginManager.FailPlugin(ass);
                    continue;
                }
                catch (TypeLoadException ex)
                {
                    Logger.LogError(ex, "Error loading types from {Assembly}.", ass.FullName);
                    _pluginManager.FailPlugin(ass);
                    continue;
                }

                foreach (var type in exportedTypes)
                {
                    if (type.IsClass && !type.IsAbstract && !type.IsInterface && !type.IsGenericType)
                    {
                        yield return type;
                    }
                }
            }
        }

        public IReadOnlyCollection<T> GetExports<T>(bool manageLifetime = true)
        {
            // Convert to list so this isn't executed for each iteration
            var parts = GetExportTypes<T>()
                .Select(CreateInstanceSafe)
                .Where(i => i != null)
                .Cast<T>()
                .ToList();

            if (manageLifetime)
            {
                foreach (var part in parts.OfType<IDisposable>())
                {
                    _ = _disposableParts.TryAdd(part, byte.MinValue);
                }
            }

            return parts;
        }

        public IReadOnlyCollection<T> GetExports<T>(CreationDelegateFactory defaultFunc, bool manageLifetime)
        {
            // Convert to list so this isn't executed for each iteration
            var parts = GetExportTypes<T>()
                .Select(i => defaultFunc(i))
                .Where(i => i != null)
                .Cast<T>()
                .ToList();

            if (manageLifetime)
            {
                foreach (var part in parts.OfType<IDisposable>())
                {
                    _ = _disposableParts.TryAdd(part, byte.MinValue);
                }
            }

            return parts;
        }

        public IEnumerable<Type> GetExportTypes<T>()
        {
            var currentType = typeof(T);
            var numberOfConcreteTypes = _allConcreteTypes.Length;
            for (var i = 0; i < numberOfConcreteTypes; i++)
            {
                var type = _allConcreteTypes[i];
                if (currentType.IsAssignableFrom(type))
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Resolves this instance.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>``0.</returns>
        public T Resolve<T>() => ServiceProvider.GetService<T>();

        public Task Shutdown()
        {
            return Task.Run(() =>
            {
                if (!_tokenSource.IsCancellationRequested)
                {
                    _tokenSource.Cancel();
                }
            });
        }
        /// <summary>
        /// Creates the instance safe.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.Object.</returns>
        protected object CreateInstanceSafe(Type type)
        {
            _creatingInstances ??= new List<Type>();

            if (_creatingInstances.Contains(type))
            {
                Logger.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName);
                foreach (var entry in _creatingInstances)
                {
                    Logger.LogError("Called from: {TypeName}", entry.FullName);
                }

                _pluginManager.FailPlugin(type.Assembly);

                throw new TypeLoadException("DI Loop detected");
            }

            try
            {
                _creatingInstances.Add(type);
                Logger.LogDebug("Creating instance of {Type}", type);
                return ActivatorUtilities.CreateInstance(ServiceProvider, type);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating {Type}", type);
                // If this is a plugin fail it.
                _pluginManager.FailPlugin(type.Assembly);
                return null;
            }
            finally
            {
                _ = _creatingInstances.Remove(type);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            if (_disposed)
            {
                return;
            }

            if (dispose)
            {
                var type = GetType();

                Logger.LogInformation("Disposing {Type}", type.Name);

                foreach (var (part, _) in _disposableParts)
                {
                    var partType = part.GetType();
                    if (partType == type)
                    {
                        continue;
                    }

                    Logger.LogInformation("Disposing {Type}", partType.Name);

                    try
                    {
                        part.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error disposing {Type}", partType.Name);
                    }
                }

                _disposableParts.Clear();
            }

            _disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Used to perform asynchronous cleanup of managed resources or for cascading calls to <see cref="DisposeAsync"/>.
        /// </summary>
        /// <returns>A ValueTask.</returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            await Task.Run(() =>
            {
                var type = GetType();

                Logger.LogInformation("Disposing {Type}", type.Name);

                foreach (var (part, _) in _disposableParts)
                {
                    var partType = part.GetType();
                    if (partType == type)
                    {
                        continue;
                    }

                    Logger.LogInformation("Disposing {Type}", partType.Name);

                    try
                    {
                        part.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error disposing {Type}", partType.Name);
                    }
                }
            });
        }
    }
}
