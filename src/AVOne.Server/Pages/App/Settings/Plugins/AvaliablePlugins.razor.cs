// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.
#nullable disable

namespace AVOne.Server.Pages.App.Settings.Plugins
{
    using AVOne.Common.Plugins;
    using AVOne.Server.Shared;
    using AVOne.Models.Updates;
    using AVOne.Configuration;
    using Masa.Blazor.Presets;
    using AVOne.Updates;

    public partial class AvaliablePlugins: ProCompontentBase
    {
        [Inject]
        private IInstallationManager InstallationManager { get; set; }

        private IEnumerable<PackageInfo> PackageInfos { 
            get{
            return _packages ?? Enumerable.Empty<PackageInfo>();
        } set{
            _packages = value;
        } }

        private IEnumerable<PackageInfo> _packages;
        
        protected override async Task OnInitializedAsync()
        {
            this._packages = await InstallationManager.GetAvailablePackages();
        }
                // create a function to get the plugin tags
        private IEnumerable<BlockTextTag> GetPackageTags(PackageInfo package)
        {
            var tags = new List<BlockTextTag>();

            if (!string.IsNullOrWhiteSpace(package.Category))
            {
                tags.Add(new BlockTextTag(package.Category, "blue", "white"));
            }

            return tags;
        }
    }
}