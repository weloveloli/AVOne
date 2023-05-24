// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Providers.Metadata
{
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Extensions;
    using AVOne.IO;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Models.Result;
    using AVOne.Providers.Metadata;
    using Microsoft.Extensions.Caching.Memory;

    public class InMemoryPornMovieMetadataProvider : ICacheMetaDataProvider<PornMovie>
    {
        private readonly IMemoryCache _cache;
        private readonly IDirectoryService _directoryService;

        public InMemoryPornMovieMetadataProvider(IDirectoryService directoryService)
        {
            this._cache = new MemoryCache(new MemoryCacheOptions());
            _directoryService = directoryService;
        }
        public string Name => "InMemory";

        public MetaDataItem? GetCache(string pid)
        {
            bool found = _cache.TryGetValue(pid, out var result);
            return found ? result as MetaDataItem : null;
        }

        public async Task<MetadataResult<PornMovie>> GetMetadata(ItemInfo info, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            var pid = info.GetProviderId(Name);
            if (string.IsNullOrWhiteSpace(pid))
            {
                return new MetadataResult<PornMovie> { HasMetadata = false };
            }
            else
            {
                var result = GetCache(pid);
                if (result != null)
                {
                    var data = new MetadataResult<PornMovie>
                    {
                        Item = new PornMovie
                        {
                            Name = result.Name,
                            Tagline = result.Tagline,
                            OriginalTitle = result.OriginalTitle,
                            OfficialRating = result.OfficialRating,
                            Overview = result.Overview,
                            Genres = result.Genres,
                            Tags = result.Tags,
                            Studios = result.Studios,
                            Path = info.Path,
                            People = result.People,
                            ProductionYear = result.ProductionYear,
                            HomePageUrl = result.HomePageUrl,
                        },
                        HasMetadata = true
                    };
                    foreach (var item in result.ImageInfos)
                    {
                        if (item.IsLocalFile)
                        {
                            data.Images.Add(new LocalImageInfo
                            {
                                FileInfo = _directoryService.GetFile(item.Path),
                                Type = item.Type
                            });
                        }
                        else
                        {
                            data.RemoteImages.Add((item.Path, item.Type));
                        }

                    }
                    return data;
                }
                else
                {
                    return new MetadataResult<PornMovie> { HasMetadata = false };
                }
            }
        }

        public void StoreCache(string pid, MetaDataItem metadata)
        {
            if (string.IsNullOrWhiteSpace(pid))
            {
                if (_cache.TryGetValue(pid, out var result))
                {
                    _cache.Remove(pid);
                }
            }
            this._cache.Set(pid, metadata);
        }
    }
}
