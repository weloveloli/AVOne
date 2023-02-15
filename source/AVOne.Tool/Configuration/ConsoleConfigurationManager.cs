﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Configuration
{
    using System;
    using Emby.Server.Implementations.AppBase;
    using MediaBrowser.Common.Configuration;
    using MediaBrowser.Model.IO;
    using MediaBrowser.Model.Serialization;
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