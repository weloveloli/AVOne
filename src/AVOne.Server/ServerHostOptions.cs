// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Server
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

        /// <inheritdoc />
        [Option("proxy", Required = false, HelpText = "proxy for example http://127.0.0.1:8000")]
        public string? Proxy { get; set; }
        public void InitService(IServiceCollection collection)
        {
        }
    }
}
