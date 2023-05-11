// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Providers
{
    using AVOne.Abstraction;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using AVOne.Providers.Download;
    using AVOne.Providers.Extractor;
    using AVOne.Providers.Metadata;
    using Microsoft.Extensions.Logging;

    public class ProviderManager : IProviderManager
    {
        private IImageProvider[] _imageProviders = Array.Empty<IImageProvider>();
        private IMetadataProvider[] _metadataProviders = Array.Empty<IMetadataProvider>();
        private INamingOptionProvider[] _namingOptionProviders = Array.Empty<INamingOptionProvider>();
        private IVideoResolverProvider[] _nameResolverProviders = Array.Empty<IVideoResolverProvider>();
        private IMetadataSaverProvider[] _metadataSaverProviders = Array.Empty<IMetadataSaverProvider>();
        private IImageSaverProvider[] _imageSaverProviders = Array.Empty<IImageSaverProvider>();
        private IDownloaderProvider[] _downloaderProviders = Array.Empty<IDownloaderProvider>();
        private IMediaExtractorProvider[] _mediaExtractorProviders = Array.Empty<IMediaExtractorProvider>();
        private readonly ILogger<ProviderManager> _logger;
        private readonly IConfigurationManager _configurationManager;
        private readonly BaseApplicationConfiguration _configuration;

        public ProviderManager(ILogger<ProviderManager> logger, IConfigurationManager configurationManager)
        {
            _logger = logger;
            _configurationManager = configurationManager;
            _configuration = configurationManager.CommonConfiguration!;
            _imageProviders = Array.Empty<IImageProvider>();
        }

        /// <inheritdoc/>
        public void AddParts(
            IEnumerable<IImageProvider> imageProviders,
            IEnumerable<IMetadataProvider> metadataProviders,
            IEnumerable<INamingOptionProvider> nameOptionProviders,
            IEnumerable<IVideoResolverProvider> resolverProviders,
            IEnumerable<IMetadataSaverProvider> metadataSaverProviders,
            IEnumerable<IImageSaverProvider> imageSaverProviders,
            IEnumerable<IDownloaderProvider> downloaderProviders,
            IEnumerable<IMediaExtractorProvider> mediaExtractorProviders
            )
        {
            _imageProviders = imageProviders.ToArray();
            _metadataProviders = metadataProviders.ToArray();
            _namingOptionProviders = nameOptionProviders.ToArray();
            _nameResolverProviders = resolverProviders.ToArray();
            _metadataSaverProviders = metadataSaverProviders.ToArray();
            _imageSaverProviders = imageSaverProviders.ToArray();
            _downloaderProviders = downloaderProviders.ToArray();
            _mediaExtractorProviders = mediaExtractorProviders.ToArray();
        }
        /// <summary>
        /// Gets the metadata providers for the provided item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="libraryOptions">The library options.</param>
        /// <typeparam name="T">The type of metadata provider.</typeparam>
        /// <returns>The metadata providers.</returns>
        public IEnumerable<IMetadataProvider<T>> GetMetadataProviders<T>(BaseItem? item)
            where T : BaseItem
        {
            var globalMetadataOptions = GetMetadataOptions(item);

            return GetMetadataProvidersInternal<T>(item, globalMetadataOptions, false, false);
        }

        public IVideoResolverProvider GetVideoResolverProvider()
        {
            return this.GetProvider(this._nameResolverProviders, _configuration.ProviderConfig.NameResolveProvider);
        }

        public INamingOptionProvider GetNamingOptionProvider()
        {
            return this.GetProvider(this._namingOptionProviders, _configuration.ProviderConfig.NameOptionProvider);
        }

        /// <inheritdoc/>
        public MetadataOptions GetMetadataOptions(BaseItem? item)
        {
            if(item == null)
            {
                return new MetadataOptions();
            }
            var type = item.GetType().Name;

            return _configurationManager.CommonConfiguration.MetadataOptions
                .FirstOrDefault(i => string.Equals(i.ItemType, type, StringComparison.OrdinalIgnoreCase)) ??
                new MetadataOptions();
        }

        private IEnumerable<IMetadataProvider<T>> GetMetadataProvidersInternal<T>(BaseItem? item, MetadataOptions globalMetadataOptions, bool includeDisabled, bool forceEnableInternetMetadata)
            where T : BaseItem
        {
            // Avoid implicitly captured closure
            var currentOptions = globalMetadataOptions;

            return _metadataProviders.OfType<IMetadataProvider<T>>()
                .OrderBy(GetDefaultOrder);
        }

        private int GetDefaultOrder(IProvider provider)
        {
            return provider is IHasOrder hasOrder ? hasOrder.Order : 0;
        }

        private T GetProvider<T>(IEnumerable<T> candidates, string name) where T : IProvider
        {
            if (string.IsNullOrEmpty(name))
            {
                return candidates.OfType<T>()
                .OrderBy(e => GetDefaultOrder(e)).First();
            }
            else
            {
                return candidates.OfType<T>().Where(e => e.Name == name)
                .OrderBy(e => GetDefaultOrder(e)).First();
            }
        }

        public IEnumerable<IImageProvider> GetImageProviders(BaseItem item)
        {
            return _imageProviders.Where(i => CanRefreshImages(i, item))
                .OrderBy(GetDefaultOrder);
        }

        public IEnumerable<IDownloaderProvider> GetDownloaderProviders(BaseDownloadableItem item)
        {
            return _downloaderProviders.OfType<IDownloaderProvider>().Where(i => i.Support(item))
                .OrderBy(GetDefaultOrder);
        }

        public IEnumerable<IMediaExtractorProvider> GetMediaExtractorProviders(string websiteUrl)
        {
            return _mediaExtractorProviders.Where(i => i.Support(websiteUrl))
                .OrderBy(GetDefaultOrder);
        }

        public IEnumerable<IMetadataSaverProvider> GetMetadataSaverProvider(BaseItem item, ItemUpdateType itemUpdateType = ItemUpdateType.None)
        {
            return _metadataSaverProviders.Where(i => i.IsEnabledFor(item, itemUpdateType))
                .OrderBy(GetDefaultOrder);
        }

        private bool CanRefreshImages(
           IImageProvider provider,
           BaseItem item)
        {
            try
            {
                if (!provider.Supports(item))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ProviderName} failed in Supports for type {ItemType} at {ItemPath}", provider.GetType().Name, item.GetType().Name, item.Path);
                return false;
            }

            return true;
        }

        public IEnumerable<IImageSaverProvider> GetImageSaverProvider()
        {
            return _imageSaverProviders.OrderBy(GetDefaultOrder);
        }

        public IMetadataProvider GetMetadataProvider(string name)
        {
            return _metadataProviders.Where(i => i.Name == name)
                .OrderBy(GetDefaultOrder).First();
        }
    }
}
