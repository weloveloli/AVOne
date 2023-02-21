// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Configuration
{
    using System;
    using AVOne.Configuration;
    using AVOne.Impl.Configuration;
    using AVOne.IO;
    using Microsoft.Extensions.Logging;

    internal class ConsoleConfigurationManager : BaseConfigurationManager
    {
        public ConsoleConfigurationManager(IApplicationPaths applicationPaths, ILoggerFactory loggerFactory, IXmlSerializer xmlSerializer, IFileSystem fileSystem) : base(applicationPaths, loggerFactory, xmlSerializer, fileSystem)
        {
            if (!File.Exists(applicationPaths.SystemConfigurationFilePath))
            {
                this.CommonConfiguration = new ConsoleConfiguration();
                this.SaveConfiguration();
            }
        }

        protected override Type ConfigurationType => typeof(ConsoleConfiguration);
    }
}
