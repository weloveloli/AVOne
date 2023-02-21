// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

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
