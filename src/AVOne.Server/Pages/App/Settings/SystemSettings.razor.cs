// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.
#nullable disable

namespace AVOne.Server.Pages.App.Settings
{
    using AVOne.Configuration;
    using AVOne.Server.Shared;

    public partial class SystemSettings : ProCompontentBase
    {
        private StringNumber _current = 0;

        [Inject]
        private IConfigurationManager ConfigurationManager { get; set; }

        [Inject]
        private IApplicationPaths ApplicationPaths { get; set; }

        private void SaveConfig()
        {
            this.ConfigurationManager.SaveConfiguration();
        }
        private void ResetConfig()
        {
            this.ConfigurationManager.ReloadConfiguration();
        }
    }
}
