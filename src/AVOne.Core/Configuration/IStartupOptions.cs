// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Configuration
{
    public interface IStartupOptions
    {
        /// <summary>
        /// Gets the value of the --ffmpeg command line option.
        /// </summary>
        string? FFmpegPath { get; }
    }
}
