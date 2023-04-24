// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Configuration
{
    using Microsoft.Extensions.DependencyInjection;

    public interface IStartupOptions
    {
        /// <summary>
        /// Gets the value of the --ffmpeg command line option.
        /// </summary>
        string? FFmpegPath { get; }

        public string? DataDir { get; set; }

        public string? Proxy { get; set; }

        public void InitService(IServiceCollection collection);
    }
}
