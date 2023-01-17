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

            if (item is Video video)
            {
                VideoType = video.VideoType;
            }

            ItemType = item.GetType();
        }

        public Type ItemType { get; set; }

        public string Path { get; set; }

        public VideoType VideoType { get; set; }
    }
}
