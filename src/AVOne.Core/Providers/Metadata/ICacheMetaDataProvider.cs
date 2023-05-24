// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Metadata
{
    using AVOne.Models.Item;

    public interface ICacheMetaDataProvider<TItemType> : ILocalMetadataProvider<TItemType>
        where TItemType : BaseItem
    {
        void StoreCache(string pid, MetaDataItem metadata);
        MetaDataItem? GetCache(string pid);
    }
}
