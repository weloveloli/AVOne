// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Migrations
{
    using AVOne.Common.Helper;
    using AVOne.Common.Migrations;
    using AVOne.Configuration;
    using AVOne.Models.Updates;

    public class AddDefaultConfig : IMigrationRoutine
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IStartupOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddMetaTubePluginRepository"/> class.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        public AddDefaultConfig(IConfigurationManager configurationManager,IStartupOptions options)
        {
            _configurationManager = configurationManager;
            _options = options;
        }

        public Guid Id => Guid.Parse("b44e41df-cd30-42f6-8284-01a9b3922631");

        public string Name => nameof(AddDefaultConfig);

        public bool PerformOnNewInstall => true;

        public void Perform()
        {
            _configurationManager.CommonConfiguration.FFmpegConfig.FFmpegPath = _options.FFmpegPath ?? ExecutableHelper.FindExecutable("ffmpeg");
            _configurationManager.SaveConfiguration();
        }
    }
}
