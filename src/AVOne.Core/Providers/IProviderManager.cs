// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers
{
    using AVOne.Configuration;
    using AVOne.Models.Item;

    public interface IProviderManager
    {
        public void AddParts(
            IEnumerable<IImageProvider> imageProviders,
            IEnumerable<IMetadataProvider> metadataProviders,
            IEnumerable<INamingOptionProvider> nameOptionProviders,
            IEnumerable<IVideoResolverProvider> resolverProviders);
        MetadataOptions GetMetadataOptions(BaseItem item);
        IEnumerable<IMetadataProvider<T>> GetMetadataProviders<T>(BaseItem item) where T : BaseItem;
        IVideoResolverProvider GetVideoResolverProvider();
        INamingOptionProvider GetNamingOptionProvider();
    }
}
