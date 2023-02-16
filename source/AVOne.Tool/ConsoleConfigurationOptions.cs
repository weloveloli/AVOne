// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool
{
    using System.Collections.Generic;

    /// <summary>
    /// Static class containing the default configuration options for the web server.
    /// </summary>
    public static class ConsoleConfigurationOptions
    {
        /// <summary>
        /// The key for the FFmpeg probe size option.
        /// </summary>
        public const string FfmpegProbeSizeKey = "FFmpeg:probesize";

        /// <summary>
        /// The key for the FFmpeg analyze duration option.
        /// </summary>
        public const string FfmpegAnalyzeDurationKey = "FFmpeg:analyzeduration";

        /// <summary>
        /// The key for the FFmpeg path option.
        /// </summary>
        public const string FfmpegPathKey = "ffmpeg";

        /// <summary>
        /// Gets a new copy of the default configuration options.
        /// </summary>
        public static Dictionary<string, string?> DefaultConfiguration => new()
        {
            { FfmpegProbeSizeKey, "1G" },
            { FfmpegAnalyzeDurationKey, "200M" }
        };
    }
}
