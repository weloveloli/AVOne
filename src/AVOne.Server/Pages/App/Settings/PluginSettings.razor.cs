// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Server.Pages.App.Settings
{
    using AVOne.Common.Plugins;
    using AVOne.Configuration;
    using AVOne.Server.Shared;
    using AVOne.Updates;

    public partial class PluginSettings : ProCompontentBase
    {
        [Inject]
        private IPluginManager PluginManager { get; set; }

        [Inject]
        private IInstallationManager InstallationManager { get; set; }

        [Inject]
        private IConfigurationManager ConfigurationManager { get; set; }

        // create a function to get all existing plugins

        private IReadOnlyCollection<LocalPlugin> InstalledPlugins => PluginManager.Plugins;

        // create a function to get all installed plugins
    }
}

