// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Models.Download
{
    using AVOne.Enum;
    using AVOne.Helper;

    public class HttpItem : BaseDownloadableItem
    {
        public HttpItem() { }
        public HttpItem(string saveName, string source, Dictionary<string, string> header, MediaQuality quality, string title)
        {
            this.SaveName = saveName;
            this.Header = header;
            this.Url = source;
            this.Quality = quality;
            this.Title = title;
        }

        public string Url { get; set; }
        public string Title { get; set; }

        public MediaQuality Quality { get; set; }

        public Dictionary<string, string> Header { get; set; }

        public override string DisplayName => $"{Title}[{Quality.Description()}]";

        public override string Key => $"HttpItem:{Url}";

        public HttpRangeSupport HttpRangeSupport { get; set; } = HttpRangeSupport.Unknown;

        public string Extension { get; set; }

        public long? Size { get; set; }
    }
}
