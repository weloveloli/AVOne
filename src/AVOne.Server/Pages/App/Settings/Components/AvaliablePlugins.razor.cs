// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.
#nullable disable

namespace AVOne.Server.Pages.App.Settings.Components
{
    using AVOne.Models.Updates;
    using AVOne.Server.Shared;
    using AVOne.Updates;
    using Masa.Blazor.Presets;

    public partial class AvaliablePlugins : ProComponentBase
    {
        [Inject]
        private IInstallationManager InstallationManager { get; set; }

        private IEnumerable<PackageInfo> PackageInfos
        {
            get
            {
                return _packages ?? Enumerable.Empty<PackageInfo>();
            }
            set
            {
                _packages = value;
            }
        }

        private IEnumerable<PackageInfo> _packages;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                this._packages = await InstallationManager.GetAvailablePackages();
            }
            catch (Exception)
            {
            }
            this._packages ??= Array.Empty<PackageInfo>();
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
