// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers
{
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Models.Item;
    using AVOne.Providers.Download;
    using AVOne.Providers.Extractor;
    using AVOne.Providers.Metadata;

    public interface IProviderManager
    {
        public void AddParts(
            IEnumerable<IImageProvider> imageProviders,
            IEnumerable<IMetadataProvider> metadataProviders,
            IEnumerable<INamingOptionProvider> nameOptionProviders,
            IEnumerable<IVideoResolverProvider> resolverProviders,
            IEnumerable<IMetadataSaverProvider> metadataSaverProviders,
            IEnumerable<IImageSaverProvider> imageSaverProviders,
            IEnumerable<IDownloaderProvider> downloaderProviders,
            IEnumerable<IMediaExtractorProvider> mediaExtractorProviders
            );

        MetadataOptions GetMetadataOptions(BaseItem item);
        IEnumerable<IMetadataProvider<T>> GetMetadataProviders<T>(BaseItem item) where T : BaseItem;
        IVideoResolverProvider GetVideoResolverProvider();
        INamingOptionProvider GetNamingOptionProvider();

        /// <summary>
        /// Gets the image providers for the provided item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="refreshOptions">The image refresh options.</param>
        /// <returns>The image providers for the item.</returns>
        IEnumerable<IImageProvider> GetImageProviders(BaseItem item);

        /// <summary>
        /// Gets the metadata saver providers for the provided item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="refreshOptions">The image refresh options.</param>
        /// <returns>The image providers for the item.</returns>
        IEnumerable<IMetadataSaverProvider> GetMetadataSaverProvider(BaseItem item, ItemUpdateType itemUpdateType = ItemUpdateType.None);

        /// <summary>
        /// Gets the image saver providers.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="refreshOptions">The image refresh options.</param>
        /// <returns>The image providers for the item.</returns>
        IEnumerable<IImageSaverProvider> GetImageSaverProvider();
        IEnumerable<IDownloaderProvider> GetDownloaderProviders(BaseDownloadableItem item);
        IEnumerable<IMediaExtractorProvider> GetMediaExtractorProviders(string websiteUrl);
    }
}
