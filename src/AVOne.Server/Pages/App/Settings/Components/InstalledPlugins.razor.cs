// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.
#nullable disable

namespace AVOne.Server.Pages.App.Settings.Components
{
    using AVOne.Common.Plugins;
    using AVOne.Configuration;
    using AVOne.Models.Updates;
    using AVOne.Server.Shared;
    using Masa.Blazor.Presets;

    public partial class InstalledPlugins : ProComponentBase
    {
        [Inject]
        private IPluginManager PluginManager { get; set; }
        // create a function to get all existing plugins
        [Inject]
        private IConfigurationManager ConfigurationManager { get; set; }
        private IReadOnlyCollection<LocalPlugin> Installed { get; set; }
        public List<RepositoryInfo> PluginRepositories { get; private set; }

        protected override void OnInitialized()
        {
            Installed = PluginManager.Plugins.ToList();
        }

        // create a function to disable plugin 
        private void DisablePlugin(LocalPlugin plugin)
        {
            // disable the plugin
            PluginManager.DisablePlugin(plugin);
            // save the configuration
            ConfigurationManager.SaveConfiguration();

            this.Success("Settings.Plugin.InstalledPlugin.Message.DisablePluginsSuccess", plugin.Name);
        }

        // create a function to enable plugin
        private void EnablePlugin(LocalPlugin plugin)
        {
            // enable the plugin
            PluginManager.EnablePlugin(plugin);
            // save the configuration
            ConfigurationManager.SaveConfiguration();

            this.Success("Settings.Plugin.InstalledPlugin.Message.EnablePluginsSuccess", plugin.Name);
        }

        // create a function to delete plugin

        private void DeletePlugin(LocalPlugin plugin)
        {
            // delete the plugin
            PluginManager.RemovePlugin(plugin);
            // save the configuration
            ConfigurationManager.SaveConfiguration();

            this.Success("Settings.Plugin.InstalledPlugin.Message.DeletePluginsSuccess", plugin.Name);
        }

        // create a function to get url of the plugin
        private static string GetPluginUrl(LocalPlugin plugin)
        {
            return $"plugins/{plugin.Id}/{plugin.Version}/Image";
        }

        // create a function to get the plugin tags
        private IEnumerable<BlockTextTag> GetPluginTags(LocalPlugin plugin)
        {
            var tags = new List<BlockTextTag>();
            switch (plugin.Manifest.Status)
            {
                case PluginStatus.Active:
                    tags.Add(new BlockTextTag(T("Common.Active"), "green", "white"));
                    break;
                case PluginStatus.Disabled:
                    tags.Add(new BlockTextTag(T("Common.Disabled"), "blue", "white"));
                    break;
                case PluginStatus.NotSupported:
                    tags.Add(new BlockTextTag(T("Common.NotSupported"), "grey", "white"));
                    break;
                case PluginStatus.Malfunctioned:
                    tags.Add(new BlockTextTag(T("Common.Malfunctioned"), "grey", "white"));
                    break;
                case PluginStatus.Restart:
                    tags.Add(new BlockTextTag(T("Common.Restart"), "grey", "white"));
                    break;
                case PluginStatus.Deleted:
                    tags.Add(new BlockTextTag(T("Common.Deleted"), "red", "white"));
                    break;
            }

            if (plugin.Manifest.AutoUpdate)
            {
                tags.Add(new BlockTextTag("AutoUpdate", "blue", "white"));
            }

            return tags;
        }

    }

}
