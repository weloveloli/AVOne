// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers
{
    using AVOne.Abstraction;
    using AVOne.Configuration;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using Microsoft.Extensions.Logging;

    public class ProviderManager : IProviderManager
    {
        private IImageProvider[] ImageProviders { get; set; }
        private IMetadataProvider[] _metadataProviders = Array.Empty<IMetadataProvider>();
        private INamingOptionProvider[] _namingOptionProviders = Array.Empty<INamingOptionProvider>();
        private readonly ILogger<ProviderManager> _logger;
        private readonly IConfigurationManager _configurationManager;

        public ProviderManager(ILogger<ProviderManager> logger, IConfigurationManager configurationManager)
        {
            this._logger = logger;
            _configurationManager = configurationManager;
            ImageProviders = Array.Empty<IImageProvider>();
        }

        /// <inheritdoc/>
        public void AddParts(
            IEnumerable<IImageProvider> imageProviders,
            IEnumerable<IMetadataProvider> metadataProviders,
            IEnumerable<INamingOptionProvider> nameOptionProviders)
        {
            ImageProviders = imageProviders.ToArray();
            _metadataProviders = metadataProviders.ToArray();
            _namingOptionProviders = nameOptionProviders.ToArray();
        }
        /// <summary>
        /// Gets the metadata providers for the provided item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="libraryOptions">The library options.</param>
        /// <typeparam name="T">The type of metadata provider.</typeparam>
        /// <returns>The metadata providers.</returns>
        public IEnumerable<IMetadataProvider<T>> GetMetadataProviders<T>(BaseItem item)
            where T : BaseItem
        {
            var globalMetadataOptions = GetMetadataOptions(item);

            return GetMetadataProvidersInternal<T>(item, globalMetadataOptions, false, false);
        }

        /// <inheritdoc/>
        public MetadataOptions GetMetadataOptions(BaseItem item)
        {
            var type = item.GetType().Name;

            return _configurationManager.CommonConfiguration.MetadataOptions
                .FirstOrDefault(i => string.Equals(i.ItemType, type, StringComparison.OrdinalIgnoreCase)) ??
                new MetadataOptions();
        }

        private IEnumerable<IMetadataProvider<T>> GetMetadataProvidersInternal<T>(BaseItem item, MetadataOptions globalMetadataOptions, bool includeDisabled, bool forceEnableInternetMetadata)
            where T : BaseItem
        {
            // Avoid implicitly captured closure
            var currentOptions = globalMetadataOptions;

            return _metadataProviders.OfType<IMetadataProvider<T>>()
                .OrderBy(GetDefaultOrder);
        }

        private int GetDefaultOrder(IMetadataProvider provider)
        {
            if (provider is IHasOrder hasOrder)
            {
                return hasOrder.Order;
            }

            return 0;
        }
    }
}
