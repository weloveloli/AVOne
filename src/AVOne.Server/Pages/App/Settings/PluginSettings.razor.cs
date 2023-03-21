// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Server.Pages.App.Settings
{
    using AVOne.Common.Plugins;
    using AVOne.Configuration;
    using AVOne.Server.Shared;
    using AVOne.Updates;
    using Masa.Blazor.Presets;
    using BlazorComponent.I18n;

    public partial class PluginSettings : ProCompontentBase
    {
        [Inject]
        private IPluginManager PluginManager { get; set; }

        [Inject]
        private IInstallationManager InstallationManager { get; set; }

        [Inject]
        private IConfigurationManager ConfigurationManager { get; set; }

        [Inject]
        private NavigationManager NavigationManager {get; set; }

        [Inject]
        private I18n I18n { get; set; }

        // create a function to get all existing plugins

        private IReadOnlyCollection<LocalPlugin> InstalledPlugins { get; set; }

        protected override void OnInitialized()
        {
            InstalledPlugins = PluginManager.Plugins.ToList();
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
            switch(plugin.Manifest.Status)
            {
                case PluginStatus.Active:
                    tags.Add(new BlockTextTag(I18n.T("Common.Active"), "green", "white"));
                    break;
                case PluginStatus.Disabled:
                    tags.Add(new BlockTextTag(I18n.T("Common.Disabled"), "blue", "white"));
                    break;
                case PluginStatus.NotSupported:
                    tags.Add(new BlockTextTag(I18n.T("Common.NotSupported"), "grey", "white"));
                    break;
                case PluginStatus.Malfunctioned:
                    tags.Add(new BlockTextTag(I18n.T("Common.Malfunctioned"), "grey", "white"));
                    break;
                case PluginStatus.Restart:
                    tags.Add(new BlockTextTag(I18n.T("Common.Restart"), "grey", "white"));
                    break;
                case PluginStatus.Deleted:
                    tags.Add(new BlockTextTag(I18n.T("Common.Deleted"), "red", "white"));
                    break;
            }

            if (plugin.Manifest.AutoUpdate)
            {
                tags.Add(new BlockTextTag("AutoUpdate", "blue", "white"));
            }

            return tags;
        }

        private void OnPluginClick(LocalPlugin plugin)
        {
            // navigate to the plugin page
            NavigationManager.NavigateTo($"app/settings/plugin/{plugin.Id}");
        }

        // create a function to disable plugin 
        private void DisablePlugin(LocalPlugin plugin)
        {
            // disable the plugin
            PluginManager.DisablePlugin(plugin);
            // save the configuration
            ConfigurationManager.SaveConfiguration();
        }


        // create a function to enable plugin
        private void EnablePlugin(LocalPlugin plugin)
        {
            // enable the plugin
            PluginManager.EnablePlugin(plugin);
            // save the configuration
            ConfigurationManager.SaveConfiguration();
        }

        // create a function to delete plugin

        private void DeletePlugin(LocalPlugin plugin)
        {
            // delete the plugin
            PluginManager.RemovePlugin(plugin);
            // save the configuration
            ConfigurationManager.SaveConfiguration();
        }

    }
}

