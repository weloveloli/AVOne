// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using Emby.Server.Implementations;

    public abstract class BaseOptions : IStartupOptions
    {
        /// <summary>
        /// Gets or sets the path to the data directory.
        /// </summary>
        /// <value>The path to the data directory.</value>
        public string? DataDir => Environment.GetEnvironmentVariable(StartupHelpers.AVOnePrefix + "DATA_DIR") ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StartupHelpers.AVONE_TOOL_NAME);
        /// <inheritdoc />
        public string? FFmpegPath { get; set; } = null;

        public bool IsService => false;

        public string? PackageName => null;

        public string? PublishedServerUrl => null;

        internal abstract Task ExecuteAsync(ConsoleAppHost host, CancellationToken token);

        /// <summary>
        /// Gets the command line options as a dictionary that can be used in the .NET configuration system.
        /// </summary>
        /// <returns>The configuration dictionary.</returns>
        public Dictionary<string, string?> ConvertToConfig()
        {
            var config = new Dictionary<string, string?>();

            if (FFmpegPath != null)
            {
                config.Add(ConsoleConfigurationOptions.FfmpegPathKey, FFmpegPath);
            }

            return config;
        }
    }
}
