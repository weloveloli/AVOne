// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Models.Download
{
    public class HttpItem : BaseDownloadableItem
    {
        public string Url { get; set; }

        public Dictionary<string, string>? Header { get; set; }

        public override string DisplayName => SaveName;

        public override string Key => $"HttpItem:{Url}";
    }
}
