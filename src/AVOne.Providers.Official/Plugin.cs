// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official
{
    using AVOne.Common.Plugins;
    using AVOne.Configuration;
    using AVOne.IO;

    public class Plugin : BasePlugin<OfficialPluginConfiguration>
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
        }

        public override string Name => "Official Plugin";

        public override Guid Id => new Guid("45e3fde0-e857-4cfe-9696-fe874ec40a2f");

        public override string Description => "Official plugin for AVOne";

    }
}
