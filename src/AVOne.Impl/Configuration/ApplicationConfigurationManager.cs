// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Configuration
{
    using System;
    using AVOne.Configuration;
    using AVOne.IO;
    using Microsoft.Extensions.Logging;

    public class ApplicationConfigurationManager : BaseConfigurationManager
    {
        public ApplicationConfigurationManager(IApplicationPaths applicationPaths, ILoggerFactory loggerFactory, IXmlSerializer xmlSerializer, IFileSystem fileSystem) : base(applicationPaths, loggerFactory, xmlSerializer, fileSystem)
        {
            if (!File.Exists(applicationPaths.SystemConfigurationFilePath))
            {
                this.CommonConfiguration = new ApplicationConfiguration();
                this.SaveConfiguration();
            }
        }
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public ApplicationConfiguration Configuration => (ApplicationConfiguration)CommonConfiguration;

        protected override Type ConfigurationType => typeof(ApplicationConfiguration);
    }
}
