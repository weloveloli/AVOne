// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Commands
{
    using AVOne.Abstraction;
    using AVOne.Tool.Resources;
    using CommandLine;

    internal abstract class BaseOptions
    {
        /// <summary>
        /// Gets or sets the path to the data directory.
        /// </summary>
        /// <value>The path to the data directory.</value>
        [Option('d', "datadir", Required = false, HelpText = "HelpTextdatadir",
            ResourceType = typeof(Resource))]
        public string? DataDir { get; set; }

        /// <inheritdoc />
        [Option("ffmpeg", Required = false, HelpText = "HelpTextffmpeg",
            ResourceType = typeof(Resource))]
        public string? FFmpegPath { get; set; }

        public abstract Task<int> ExecuteAsync(IServiceProvider provider, CancellationToken token);

        /// <summary>
        /// Gets the command line options as a dictionary that can be used in the .NET configuration system.
        /// </summary>
        /// <returns>The configuration dictionary.</returns>
        public Dictionary<string, string?> ConvertToConfig()
        {
            var config = new Dictionary<string, string?>();

            if (FFmpegPath != null)
            {
                config.Add(ConfigurationOptions.FfmpegPathKey, FFmpegPath);
            }

            return config;
        }
    }
}
