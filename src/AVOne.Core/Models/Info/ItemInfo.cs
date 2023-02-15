// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Models.Info
{
    using System;
    using AVOne.Enum;
    using AVOne.Models.Item;

    public class ItemInfo
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
        }

        public Type ItemType { get; set; }

        public string Path { get; set; }

        public VideoType VideoType { get; set; }

        public string ContainingFolderPath { get; set; }

        public bool IsInMixedFolder { get; set; }

        public bool IsPlaceHolder { get; set; }
    }
}
