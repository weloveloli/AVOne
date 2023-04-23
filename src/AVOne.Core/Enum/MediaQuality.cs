// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Enum
{
    using System.ComponentModel;

    // An enum class to represent different media quality levels
    public enum MediaQuality
    {
        // not know
        [Description("")]
        None = 0,

        // Low quality: 240p resolution, 64 kbps audio bitrate
        [Description("240p")]
        Low = 1,

        // Medium quality: 480p resolution, 128 kbps audio bitrate
        [Description("480p")]
        Medium = 2,

        // High quality: 720p resolution, 192 kbps audio bitrate
        [Description("720p")]
        High = 3,

        // Very high quality: 1080p resolution, 256 kbps audio bitrate
        [Description("1080p")]
        VeryHigh = 4,

        // Ultra high quality: 4K resolution, 320 kbps audio bitrate
        [Description("4K")]
        UltraHigh = 5
    }
}
