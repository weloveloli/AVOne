// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Jellyfin
{
    using AVOne.Common.Plugins;
    using AVOne.Configuration;
    using AVOne.IO;

    public class Plugin : BasePlugin<JellyfinConfiguration>
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
        }

        public override string Name => "Jellyfin";
    }
}
