namespace AVOne.Plugin.Template
{
    using System;
    using AVOne.Common.Plugins;
    using AVOne.Configuration;
    using AVOne.IO;

    public class Plugin : BasePlugin<PluginConfiguration>
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
        }

        public override string Name => throw new NotImplementedException();
    }
}
