// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers
{
    using AVOne.Configuration;
    using AVOne.Models.Item;

    public interface IProviderManager
    {
        void AddParts(IEnumerable<IImageProvider> imageProviders, IEnumerable<IMetadataProvider> metadataProviders, IEnumerable<INamingOptionProvider> nameOptionProviders);
        MetadataOptions GetMetadataOptions(BaseItem item);
        IEnumerable<IMetadataProvider<T>> GetMetadataProviders<T>(BaseItem item) where T : BaseItem;
    }
}
