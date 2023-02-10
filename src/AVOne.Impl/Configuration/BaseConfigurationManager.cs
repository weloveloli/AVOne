// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Configuration
{
    using AVOne.Configuration;
    using AVOne.Helper;
    using AVOne.IO;
    using Microsoft.Extensions.Logging;

    public abstract class BaseConfigurationManager : IConfigurationManager
    {
        private readonly IFileSystem _fileSystem;

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

        public IApplicationPaths ApplicationPaths { get; set; }

        /// <summary>
        /// The _configuration.
        /// </summary>
        private BaseApplicationConfiguration? _configuration;

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
#pragma warning disable CS8603 // Possible null reference return.
                    return _configuration;
#pragma warning restore CS8603 // Possible null reference return.
                }

                lock (_configurationSyncLock)
                {
                    if (_configurationLoaded)
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        return _configuration;
#pragma warning restore CS8603 // Possible null reference return.
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

        public BaseConfigurationManager(IApplicationPaths applicationPaths, ILoggerFactory loggerFactory, IXmlSerializer xmlSerializer, IFileSystem fileSystem)
        {
            XmlSerializer = xmlSerializer;
            _fileSystem = fileSystem;
            ApplicationPaths = applicationPaths;
            Logger = loggerFactory.CreateLogger<BaseConfigurationManager>();
        }
    }
}
