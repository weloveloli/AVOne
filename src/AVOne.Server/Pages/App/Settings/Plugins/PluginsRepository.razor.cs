﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.
#nullable disable

namespace AVOne.Server.Pages.App.Settings.Plugins
{
    using AVOne.Server.Shared;
    using AVOne.Models.Updates;
    using AVOne.Configuration;

    public partial class PluginsRepository : ProCompontentBase
    {
        [Inject]
        private IConfigurationManager ConfigurationManager { get; set; }
        public List<RepositoryInfo> PluginRepositories { get; private set; }
        protected override void OnInitialized()
        {
            this.PluginRepositories = ConfigurationManager.CommonConfiguration.PluginRepositories;
        }
        // create a function to get the plugin tags

        private void AddPluginRepo(AddRepoModel model)
        {

            if (ConfigurationManager.CommonConfiguration.PluginRepositories.Any(x => x.Url == model.RepoUrl))
            {
                ShowSnackbarLocal("Settings.Plugin.PluginRepository.Message.AddRepoFailed", model.Name);
                return;
            }
            ConfigurationManager.CommonConfiguration.PluginRepositories.Add(new RepositoryInfo
            {
                Name = model.Name,
                Url = model.RepoUrl
            });
            // save the configuration
            ConfigurationManager.SaveConfiguration();
            ShowSnackbarLocal("Settings.Plugin.PluginRepository.Message.AddRepoSuccess", model.Name);
        }

        private void RemovePluginRepo(RepositoryInfo toRemove)
        {
            ConfigurationManager.CommonConfiguration.PluginRepositories.Remove(toRemove);
            // save the configuration
            ConfigurationManager.SaveConfiguration();
            ShowSnackbarLocal("Settings.Plugin.PluginRepository.Message.DeleteRepoSuccess", toRemove.Name);
        }

        private void UpdatePluginRepo(RepositoryInfo toUpdate)
        {
            toUpdate.Enabled = !toUpdate.Enabled;
            // save the configuration
            ConfigurationManager.SaveConfiguration();
            if (toUpdate.Enabled)
            {
                ShowSnackbarLocal("Settings.Plugin.PluginRepository.Message.EnableRepoSuccess", toUpdate.Name);
            }
            else
            {
                ShowSnackbarLocal("Settings.Plugin.PluginRepository.Message.DisableRepoSuccess", toUpdate.Name);
            }
        }
    }
}