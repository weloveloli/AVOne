// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Models.Download
{
    using AVOne.Enum;
    using AVOne.Helper;

    public class M3U8Item : BaseDownloadableItem
    {
        public M3U8Item()
        {
        }
        public M3U8Item(string saveName, string url, Dictionary<string, string> header, MediaQuality quality, string title)
        {
            SaveName = saveName;
            Url = url;
            Header = header;
            Quality = quality;
            Title = title;
        }

        public string Url { get; set; }
        public Dictionary<string, string> Header { get; set; }

        public MediaQuality Quality { get; set; } = MediaQuality.Low;

        public string Title { get; set; }

        public override string DisplayName => $"[{Quality.Description()}]{Title}";

        public override string Key => $"M3U8Item:{Url}";

    }
}
