// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using AVOne.Common.Helper;
    using AVOne.Configuration;
    using CommandLine;
    using Microsoft.Extensions.DependencyInjection;

    internal class ServerHostOptions : IStartupOptions
    {
        /// <summary>
        /// Gets or sets the path to the data directory.
        /// </summary>
        /// <value>The path to the data directory.</value>
        [Option("data-dir", Required = false, HelpText = "data path")]
        public string? DataDir { get; set; }

        /// <inheritdoc />
        [Option("ffmpeg", Required = false, HelpText = "ffmpeg path")]
        public string? FFmpegPath { get; set; } = ExecutableHelper.FindExecutable("ffmpeg");

        public void InitService(IServiceCollection collection)
        {
        }
    }
}
