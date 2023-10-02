// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Plugins.MetaTube
{
    using System;
    using AVOne.Common.Plugins;
    using AVOne.Configuration;
    using AVOne.IO;
    using AVOne.Plugins.MetaTube.Configuration;

    public class Plugin : BasePlugin<MetaTubeConfiguration>
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override string Name => "MetaTube";

        public override string Description => "Metadata Tube Plugin for AVOne";

        public override Guid Id => Guid.Parse("01cc53ec-c415-4108-bbd4-a684a9801a32");

        public static Plugin Instance { get; private set; }
    }
}
