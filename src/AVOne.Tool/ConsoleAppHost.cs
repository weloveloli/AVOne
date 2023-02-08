﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable

namespace AVOne.Tool
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using AVOne.Abstraction;
    using AVOne.Configuration;
    using AVOne.Impl.Constants;
    using AVOne.Impl.IO;
    using AVOne.Impl.Providers;
    using AVOne.IO;
    using AVOne.Providers;
    using AVOne.Tool.Commands;
    using AVOne.Tool.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;

    internal class ConsoleAppHost : IApplicationHost, IAsyncDisposable, IDisposable
    {
        private readonly BaseOptions _option;
        private readonly CancellationTokenSource _tokenSource;
        private readonly ConsoleApplicationPaths _appPaths;
        private bool _disposed = false;
        private List<Type> _creatingInstances;
        /// <summary>
        /// The disposable parts.
        /// </summary>
        private readonly ConcurrentDictionary<IDisposable, byte> _disposableParts = new();

        private readonly IXmlSerializer _xmlSerializer;

        private readonly IFileSystem _fileSystem;

        public Version ApplicationVersion { get; }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        public ConsoleApplicationPaths AppPaths => _appPaths;

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

        public ConsoleAppHost(BaseOptions option, ILoggerFactory loggerFactory, CancellationTokenSource tokenSource, ConsoleApplicationPaths path)
        {
            this._option = option;
            LoggerFactory = loggerFactory;
            _tokenSource = tokenSource;
            ApplicationVersion = typeof(ConsoleAppHost).Assembly.GetName().Version;
            _appPaths = path;
            Logger = loggerFactory.CreateLogger<ConsoleAppHost>();
            _fileSystem = new ManagedFileSystem(LoggerFactory.CreateLogger<ManagedFileSystem>(), path);
            _xmlSerializer = new DefaultXmlSerializer();
            ConfigurationManager = new ConsoleConfigurationManager(path,loggerFactory, _xmlSerializer, _fileSystem);
        }

        public async Task<int> ExecuteCmd()
        {
            return await _option.ExecuteAsync(this, _tokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Discovers the types.
        /// </summary>
        protected void DiscoverTypes()
        {
            Logger.LogInformation("Loading assemblies");
            _allConcreteTypes = GetTypes(GetComposablePartAssemblies()).ToArray();
        }

        public void Init(IServiceCollection serviceCollection)
        {
            DiscoverTypes();
            serviceCollection.TryAdd(ServiceDescriptor.Singleton(LoggerFactory));
            serviceCollection.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
            _ = serviceCollection.AddHttpClient();
            RegisterServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
            AddParts();
        }

        private void AddParts()
        {
            Resolve<IProviderManager>().AddParts(
                GetExports<IImageProvider>(),
                GetExports<IMetadataProvider>(),
                GetExports<INamingOptionProvider>());
        }

        /// <summary>
        /// Registers services/resources with the service collection that will be available via DI.
        /// </summary>
        /// <param name="serviceCollection">Instance of the <see cref="IServiceCollection"/> interface.</param>
        protected void RegisterServices(IServiceCollection serviceCollection)
        {
            _option.InitService(serviceCollection);
            serviceCollection.AddMemoryCache();
            serviceCollection.AddSingleton<IApplicationHost>(this);
            serviceCollection.AddSingleton<IStartupOptions>(this._option);
            serviceCollection.AddSingleton<IApplicationPaths>(AppPaths);
            serviceCollection.AddSingleton(this._xmlSerializer);
            serviceCollection.AddSingleton(this._fileSystem);
            serviceCollection.AddSingleton<IConfigurationManager>(ConfigurationManager);
            serviceCollection.AddSingleton<IProviderManager, ProviderManager>();
        }

        /// <summary>
        /// Gets the composable part assemblies.
        /// </summary>
        /// <returns>IEnumerable{Assembly}.</returns>
        protected IEnumerable<Assembly> GetComposablePartAssemblies()
        {
            // Include composable parts in the AVOne.Impl assembly
            yield return typeof(OfficialProviderNames).Assembly;
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
                    continue;
                }
                catch (TypeLoadException ex)
                {
                    Logger.LogError(ex, "Error loading types from {Assembly}.", ass.FullName);
                    continue;
                }

                foreach (Type type in exportedTypes)
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
                    _disposableParts.TryAdd(part, byte.MinValue);
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
                    _disposableParts.TryAdd(part, byte.MinValue);
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
                return null;
            }
            finally
            {
                _creatingInstances.Remove(type);
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