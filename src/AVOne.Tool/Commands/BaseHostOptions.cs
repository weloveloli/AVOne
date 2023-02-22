// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using AVOne.Configuration;
    using AVOne.Tool.Resources;
    using CommandLine;
    using Microsoft.Extensions.DependencyInjection;

    internal abstract class BaseHostOptions : IStartupOptions
    {
        /// <summary>
        /// Gets or sets the path to the data directory.
        /// </summary>
        /// <value>The path to the data directory.</value>
        [Option("data-dir", Required = false, HelpText = "HelpTextdatadir",
            ResourceType = typeof(Resource))]
        public string? DataDir { get; set; }

        /// <inheritdoc />
        [Option("ffmpeg", Required = false, HelpText = "HelpTextffmpeg",
            ResourceType = typeof(Resource))]
        public string? FFmpegPath { get; set; }

        public virtual void InitService(IServiceCollection collection)
        {

        }

        public abstract Task ExecuteAsync(ConsoleAppHost host, CancellationToken token);

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
