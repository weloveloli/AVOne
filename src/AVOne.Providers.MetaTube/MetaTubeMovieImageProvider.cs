// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers.Metatube
{
    using AVOne.Abstraction;
    using AVOne.Enum;
    using AVOne.Impl.Configuration;
    using AVOne.Impl.Extensions;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using Microsoft.Extensions.Logging;

    public class MetaTubeMovieImageProvider : BaseProvider, IRemoteImageProvider, IHasOrder
    {

        public MetaTubeMovieImageProvider(ILogger<MetatubeMovieMetaDataProvider> logger,
                                             IOfficialProvidersConfiguration officialProvidersConfiguration,
                                             MetatubeApiClient metatubeApiClient)
            : base(logger, officialProvidersConfiguration, metatubeApiClient)
        {
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var pid = item.GetPid(Name);
            if (string.IsNullOrWhiteSpace(pid.Id) || string.IsNullOrWhiteSpace(pid.Provider))
            {
                return Enumerable.Empty<RemoteImageInfo>();
            }

            var m = await ApiClient.GetMovieInfoAsync(pid.Provider, pid.Id, cancellationToken);
            var images = new List<RemoteImageInfo>
        {
            new()
            {
                ProviderName = Name,
                Type = ImageType.Primary,
                Url = ApiClient.GetPrimaryImageApiUrl(m.Provider, m.Id, pid.Position ?? -1)
            },
                new()
            {
                ProviderName = Name,
                Type = ImageType.Thumb,
                Url = ApiClient.GetThumbImageApiUrl(m.Provider, m.Id)
            },
            new()
            {
                ProviderName = Name,
                Type = ImageType.Backdrop,
                Url = ApiClient.GetBackdropImageApiUrl(m.Provider, m.Id)
            }
        };

            foreach (var imageUrl in m.PreviewImages ?? Enumerable.Empty<string>())
            {
                images.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Primary,
                    Url = ApiClient.GetPrimaryImageApiUrl(m.Provider, m.Id, imageUrl, pid.Position ?? -1)
                });

                images.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Thumb,
                    Url = ApiClient.GetThumbImageApiUrl(m.Provider, m.Id, imageUrl)
                });

                images.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Backdrop,
                    Url = ApiClient.GetBackdropImageApiUrl(m.Provider, m.Id, imageUrl)
                });
            }

            return images;
        }

        public bool Supports(BaseItem item)
        {
            return item is PornMovie;
        }

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new List<ImageType>
        {
            ImageType.Primary,
            ImageType.Thumb,
            ImageType.Backdrop
        };
        }
    }
}
