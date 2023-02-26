// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable
namespace AVOne.Impl.Configuration
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using AVOne.Configuration;
    using AVOne.Helper;
    using AVOne.IO;
    using Furion.FriendlyException;
    using Microsoft.Extensions.Logging;

    public abstract class BaseConfigurationManager : IConfigurationManager
    {
        private readonly IFileSystem _fileSystem;

        private readonly ConcurrentDictionary<string, object> _configurations = new ConcurrentDictionary<string, object>();

        private ConfigurationStore[] _configurationStores = Array.Empty<ConfigurationStore>();
        private IConfigurationFactory[] _configurationFactories = Array.Empty<IConfigurationFactory>();

        /// <summary>
        /// The _configuration loaded.
        /// </summary>
        private bool _configurationLoaded;

        /// <summary>
        /// The _configuration sync lock.
        /// </summary>
        private readonly object _configurationSyncLock = new();

        /// <summary>
        /// Gets the type of the configuration.
        /// </summary>
        /// <value>The type of the configuration.</value>
        protected abstract Type ConfigurationType { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        protected ILogger<BaseConfigurationManager> Logger { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseConfigurationManager"/> class.
        /// </summary>
        /// <param name="applicationPaths">The application paths.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="xmlSerializer">The XML serializer.</param>
        /// <param name="fileSystem">The file system.</param>
        public BaseConfigurationManager(IApplicationPaths applicationPaths, ILoggerFactory loggerFactory, IXmlSerializer xmlSerializer, IFileSystem fileSystem)
        {
            XmlSerializer = xmlSerializer;
            _fileSystem = fileSystem;
            ApplicationPaths = applicationPaths;
            Logger = loggerFactory.CreateLogger<BaseConfigurationManager>();
        }

        public IApplicationPaths ApplicationPaths { get; set; }

        /// <summary>
        /// The _configuration.
        /// </summary>
        private BaseApplicationConfiguration _configuration;

        /// <summary>
        /// Gets or sets the system configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public BaseApplicationConfiguration CommonConfiguration
        {
            get
            {
                if (_configurationLoaded)
                {
                    return _configuration;
                }

                lock (_configurationSyncLock)
                {
                    if (_configurationLoaded)
                    {
                        return _configuration;
                    }

                    _configuration = (BaseApplicationConfiguration)ConfigurationHelper.GetXmlConfiguration(ConfigurationType, ApplicationPaths.SystemConfigurationFilePath, XmlSerializer);
                    _configurationLoaded = true;

                    return _configuration;
                }
            }

            protected set
            {
                _configuration = value;

                _configurationLoaded = value != null;
            }
        }

        /// <summary>
        /// Gets the XML serializer.
        /// </summary>
        /// <value>The XML serializer.</value>
        protected IXmlSerializer XmlSerializer { get; private set; }

        public void SaveConfiguration()
        {
            Logger.LogInformation("Saving system configuration");
            var path = ApplicationPaths.SystemConfigurationFilePath;
            lock (_configurationSyncLock)
            {
                XmlSerializer.SerializeToFile(CommonConfiguration, path);
            }
        }

        public void ReplaceConfiguration(BaseApplicationConfiguration newConfiguration)
        {
            throw new NotImplementedException();
        }

        public void RegisterConfiguration<T>() where T : IConfigurationFactory
        {
            IConfigurationFactory factory = Activator.CreateInstance<T>();

            if (_configurationFactories is null)
            {
                _configurationFactories = new[] { factory };
            }
            else
            {
                var oldLen = _configurationFactories.Length;
                var arr = new IConfigurationFactory[oldLen + 1];
                _configurationFactories.CopyTo(arr, 0);
                arr[oldLen] = factory;
                _configurationFactories = arr;
            }

            _configurationStores = _configurationFactories
                .SelectMany(i => i.GetConfigurations())
                .ToArray();
        }

        public ConfigurationStore[] GetConfigurationStores()
        {
            return _configurationStores;
        }

        public void SaveConfiguration(string key, object configuration)
        {
            var configurationStore = GetConfigurationStore(key);
            var configurationType = configurationStore.ConfigurationType;

            if (configuration.GetType() != configurationType)
            {
                throw new ArgumentException("Expected configuration type is " + configurationType.Name);
            }

            _configurations.AddOrUpdate(key, configuration, (_, _) => configuration);

            var path = GetConfigurationFile(key);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Path can't be a root directory."));

            lock (_configurationSyncLock)
            {
                XmlSerializer.SerializeToFile(configuration, path);
            }

        }

        public void AddParts(IEnumerable<IConfigurationFactory> factories)
        {
            _configurationFactories = factories.ToArray();

            _configurationStores = _configurationFactories
                .SelectMany(i => i.GetConfigurations())
                .ToArray();
        }
        private ConfigurationStore GetConfigurationStore(string key)
        {
            return _configurationStores
                .First(i => string.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase));
        }
        /// <inheritdoc />
        public object GetConfiguration(string key)
        {
            return _configurations.GetOrAdd(
                key,
                static (k, configurationManager) =>
                {
                    var file = configurationManager.GetConfigurationFile(k);

                    var configurationInfo = Array.Find(
                        configurationManager._configurationStores,
                        i => string.Equals(i.Key, k, StringComparison.OrdinalIgnoreCase));

                    if (configurationInfo is null)
                    {
                        throw Oops.Oh("Configuration with key " + k + " not found.");
                    }

                    var configurationType = configurationInfo.ConfigurationType;

                    lock (configurationManager._configurationSyncLock)
                    {
                        return configurationManager.LoadConfiguration(file, configurationType);
                    }
                },
                this);
        }

        private object LoadConfiguration(string path, Type configurationType)
        {
            try
            {
                if (File.Exists(path))
                {
                    return XmlSerializer.DeserializeFromFile(configurationType, path);
                }
            }
            catch (Exception ex) when (ex is not IOException)
            {
                Logger.LogError(ex, "Error loading configuration file: {Path}", path);
            }

            return Activator.CreateInstance(configurationType)
                ?? throw Oops.Oh("Configuration type can't be Nullable<T>.");
        }

        private string GetConfigurationFile(string key)
        {
            return Path.Combine(ApplicationPaths.ConfigurationDirectoryPath, key.ToLowerInvariant() + ".xml");
        }
    }
}
