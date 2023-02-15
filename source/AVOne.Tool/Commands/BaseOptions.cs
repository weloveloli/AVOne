// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Commands
{
    using CommandLine;
    using Emby.Server.Implementations;
    using Microsoft.Extensions.Hosting;

    public abstract class BaseOptions : IStartupOptions
    {
        /// <summary>
        /// Gets or sets the path to the data directory.
        /// </summary>
        /// <value>The path to the data directory.</value>
        [Option('d', "data-dir", Required = false, HelpText = "Path to the data directory.")]
        public string? DataDir { get; set; }
        /// <inheritdoc />
        [Option("ffmpeg", Required = false, HelpText = "Path to external FFmpeg executable to use in place of default found in PATH.")]
        public string? FFmpegPath => throw new NotImplementedException();

        public bool IsService => false;

        public string? PackageName => null;

        public string? PublishedServerUrl => null;

        internal abstract Task<int> ExecuteAsync(ConsoleAppHost host, CancellationToken token);
    }
}
