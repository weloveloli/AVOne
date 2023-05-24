// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Models.Info
{
    using System;
    using System.Collections.Generic;
    using AVOne.Abstraction;
    using AVOne.Enum;
    using AVOne.Models.Item;

    public class ItemInfo : IHasProviderIds
    {
        public ItemInfo(BaseItem item)
        {
            Path = item.Path;
            ContainingFolderPath = item.ContainingFolderPath;
            IsInMixedFolder = item.IsInMixedFolder;
            if (item is Video video)
            {
                VideoType = video.VideoType;
            }

            ItemType = item.GetType();
            ProviderIds = item.ProviderIds;
        }

        public Type ItemType { get; set; }

        public string Path { get; set; }

        public VideoType VideoType { get; set; }

        public string ContainingFolderPath { get; set; }

        public bool IsInMixedFolder { get; set; }

        public bool IsPlaceHolder { get; set; }

        public Dictionary<string, string> ProviderIds { get; set; }
    }
}
