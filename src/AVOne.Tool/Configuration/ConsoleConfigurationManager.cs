﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

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
        }

        protected override Type ConfigurationType => typeof(ConsoleConfiguration);
    }
}