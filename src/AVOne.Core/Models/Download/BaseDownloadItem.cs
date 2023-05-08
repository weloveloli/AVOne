// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable
namespace AVOne.Models.Download
{
    using AVOne.Models.Item;

    public abstract class BaseDownloadableItem : MetaDataItem
    {
        public string SaveName { get; set; }
        public virtual string DisplayName { get; set; }
        public abstract string Key { get; }
        public string OrignalLink { get; set; }
        public bool HasMetaData { get; set; } = false;
    }
}
