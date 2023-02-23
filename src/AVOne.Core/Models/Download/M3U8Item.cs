// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Models.Download
{
    public class M3U8Item : BaseDownloadableItem
    {
        public string Url { get; set; }
        public Dictionary<string, string> Header { get; set; }
    }
}
