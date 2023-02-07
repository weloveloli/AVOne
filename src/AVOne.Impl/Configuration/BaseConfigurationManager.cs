// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Configuration
{
    using AVOne.Configuration;
    using AVOne.IO;
    using Microsoft.Extensions.Logging;

    public class BaseConfigurationManager : IConfigurationManager
    {
        /// <summary>
        /// The _configuration loaded.
        /// </summary>
        private bool _configurationLoaded;

        public IApplicationPaths CommonApplicationPaths { get; set; }

        public IApplicationConfigs Configuration { get; set; }

        public void SaveConfiguration()
        {
            throw new NotImplementedException();
        }

        public BaseConfigurationManager(IApplicationPaths applicationPaths, ILoggerFactory loggerFactory, IXmlSerializer xmlSerializer, IFileSystem fileSystem)
        {

        }
    }
}
