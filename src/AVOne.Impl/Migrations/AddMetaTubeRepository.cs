// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Migrations
{
    using System;
    using AVOne.Common.Migrations;
    using AVOne.Configuration;
    using AVOne.Models.Updates;

    public class AddMetaTubePluginRepository : IMigrationRoutine
    {
        private readonly IConfigurationManager _configurationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddMetaTubePluginRepository"/> class.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        public AddMetaTubePluginRepository(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        private readonly RepositoryInfo _defaultRepositoryInfo = new RepositoryInfo
        {
            Name = "AVOne Official",
            Url = "https://raw.githubusercontent.com/weloveloli/AVOne.Plugins/dist/manifest.json"
        };

        public Guid Id => Guid.Parse("F9C6E0D8-7B4F-4E3D-A5A9-6C2B0B8F7E5A");

        public string Name => nameof(AddMetaTubePluginRepository);

        public bool PerformOnNewInstall => true;

        public void Perform()
        {
            var repositories = _configurationManager.CommonConfiguration.PluginRepositories ?? new List<RepositoryInfo>();
            repositories.Add(_defaultRepositoryInfo);
            _configurationManager.SaveConfiguration();
        }
    }
}
