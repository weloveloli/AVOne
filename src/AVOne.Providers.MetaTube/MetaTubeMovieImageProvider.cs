// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Metatube
{
    using AVOne.Abstraction;
    using AVOne.Common.Enum;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Common.Extensions;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using Furion.FriendlyException;
    using Microsoft.Extensions.Logging;
    using AVOne.Providers.Metadata;

    public class MetaTubeMovieImageProvider : BaseProvider, IRemoteImageProvider, IHasOrder
    {

        public MetaTubeMovieImageProvider(ILogger<MetatubeMovieMetaDataProvider> logger,
                                             IConfigurationManager configurationManager,
                                             MetatubeApiClient metatubeApiClient)
            : base(logger, configurationManager, metatubeApiClient)
        {
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            if (!IsProviderAvailable())
            {
                throw Oops.Oh(ErrorCodes.PROVIDER_NOT_AVAILABLE, Name);
            }

            var pid = item.GetPid(this.Name);
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
            if (!IsProviderAvailable())
            {
                return false;
            }

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
