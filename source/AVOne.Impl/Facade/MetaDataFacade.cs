// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Facade
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.Intrinsics.Arm;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Impl.Configuration;
    using AVOne.Impl.Models;
    using Emby.Server.Implementations;
    using Furion.FriendlyException;
    using MediaBrowser.Common.Configuration;
    using MediaBrowser.Controller;
    using MediaBrowser.Controller.Entities;
    using MediaBrowser.Controller.Entities.Movies;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Controller.Providers;
    using MediaBrowser.Model.Configuration;
    using MediaBrowser.Model.Entities;
    using MediaBrowser.Model.IO;
    using MediaBrowser.Model.Net;
    using Microsoft.Extensions.Logging;
    using Nikse.SubtitleEdit.Core.SubtitleFormats;

    public class MetaDataFacade : IMetaDataFacade
    {
        private readonly ILogger<MetaDataFacade> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IProviderManager _providerManager;
        private readonly IDirectoryService _directoryService;
        private readonly IConfigurationManager _configurationManager;
        private readonly IServerApplicationPaths _serverApplicationPaths;

        private AVOneConfiguration _config => _configurationManager
            .GetConfiguration<AVOneConfiguration>(AVOneConfigStore.StoreKey);

        public MetaDataFacade(
            ILogger<MetaDataFacade> logger,
            ILibraryManager libraryManager,
            IProviderManager providerManager,
            IDirectoryService directoryService,
            IConfigurationManager configurationManager,
            IServerApplicationPaths serverApplicationPaths
            )
        {
            _logger = logger;
            _libraryManager = libraryManager;
            _providerManager = providerManager;
            _directoryService = directoryService;
            _configurationManager = configurationManager;
            _serverApplicationPaths = serverApplicationPaths;
        }
        public async Task<MoveMetaDataItem> ResolveAsMovie(string path, CancellationToken token = default)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            var fileInfo = _directoryService.GetFile(path);
            var parent = new Folder { Path = Directory.GetParent(path).FullName };
            var items = _libraryManager.ResolvePaths(
                new List<FileSystemMetadata> { fileInfo },
                _directoryService,
                parent,
                new LibraryOptions(),
                CollectionType.Movies);
            var movies = await GetMovie(items, token);
            return movies.FirstOrDefault();
        }

        public Task<IEnumerable<MoveMetaDataItem>> ResolveAsMovies(string dir, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                return Task.FromResult(Enumerable.Empty<MoveMetaDataItem>());
            }
            var fileInfos = _directoryService.GetFiles(dir);
            var parent = new Folder { Path = dir };
            var items = _libraryManager.ResolvePaths(
                fileInfos,
                _directoryService,
                parent,
                new LibraryOptions(),
                CollectionType.Movies);
            return GetMovie(items, token);
        }

        private async Task<IEnumerable<MoveMetaDataItem>> GetMovie(IEnumerable<BaseItem> items, CancellationToken token = default)
        {
            if (items == null || !items.Any())
            {
                return Enumerable.Empty<MoveMetaDataItem>();
            }

            var movis = items.OfType<Movie>().Select(movieItem =>
            {
                var m = new MoveMetaDataItem { Movie = movieItem };
                var libOpt = new LibraryOptions();
                var providers = _providerManager
                    .GetMetadataProviders<Movie>(movieItem, libOpt)
                    .Where(e => _config.ScanMetaDataProviders.Contains(e.Name));
                m.LocalMetadataProvider = providers.OfType<ILocalMetadataProvider<Movie>>().FirstOrDefault();
                m.RemoteMetadataProvider = providers.OfType<IRemoteMetadataProvider<Movie, MovieInfo>>().FirstOrDefault();
                var imageProviders = _providerManager.GetImageProviders(movieItem, new ImageRefreshOptions(_directoryService));
                m.LocalImageProvider = imageProviders.OfType<ILocalImageProvider>().FirstOrDefault();
                m.RemoteImageProvider = imageProviders
                .OfType<IRemoteImageProvider>()
                .Where(e => _config.ImageMetaDataProviders.Contains(e.Name))
                .FirstOrDefault();
                return m;
            });

            var tasks = movis.Select(e => ExecuteMetaData(e, token));

            var movieItems = await Task.WhenAll(tasks);

            return movieItems.Select(this.MergeMetaData);
        }

        private async Task<MoveMetaDataItem> ExecuteMetaData(MoveMetaDataItem dataItem, CancellationToken token = default)
        {
            var item = new MoveMetaDataItem
            {
                Movie = dataItem.Movie,
                HasMetaData = false,
                LocalImageProvider = dataItem.LocalImageProvider,
                RemoteImageProvider = dataItem.RemoteImageProvider,
                LocalMetadataProvider = dataItem.LocalMetadataProvider,
                RemoteMetadataProvider = dataItem.RemoteMetadataProvider
            };

            var metaDataResult = await item.LocalMetadataProvider
                .GetMetadata(new ItemInfo(item.Movie), _directoryService, token);
            if (metaDataResult.HasMetadata)
            {
                item.HasMetaData = true;
                item.MetadataResult = metaDataResult;
                item.Result = metaDataResult.Item;
            }
            else
            {
                try
                {
                    metaDataResult = await item.RemoteMetadataProvider.GetMetadata(item.Movie.GetLookupInfo(), token);
                    if (metaDataResult.HasMetadata)
                    {
                        item.HasMetaData = true;
                        item.MetadataResult = metaDataResult;
                        item.Result = metaDataResult.Item;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "Fetch metadata for media {0} error", item.Movie.Name);
                }

            }
            if (!item.HasMetaData)
            {
                return item;
            }
            item.LocalImageInfos = item.LocalImageProvider.GetImages(item.Result, _directoryService);
            item.RemoteImageInfos = await item.RemoteImageProvider.GetImages(item.Result, token);
            var hash = SHA256.Create();
            if (item.RemoteImageInfos != null && item.RemoteImageInfos.Any())
            {
                var imagsList = new List<LocalImageInfo>();
                item.LocalRemoteImageInfos = imagsList;
                foreach (var image in item.RemoteImageInfos)
                {
                    if(image.Type != ImageType.Primary)
                    {
                        continue;
                    }

                    var imageHash = string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(image.Url)).Select(x => x.ToString("x2")));
                    var imagePng = Path.Combine(_serverApplicationPaths.ImageCachePath, imageHash + ".png");
                    var imageJpg = Path.Combine(_serverApplicationPaths.ImageCachePath, imageHash + ".jpg");
                    if (File.Exists(imageJpg))
                    {
                        imagsList.Add(new LocalImageInfo
                        {
                            FileInfo = _directoryService.GetFile(imageJpg),
                            Type = image.Type
                        });
                        continue;
                    }
                    else if (File.Exists(imagePng))
                    {
                        imagsList.Add(new LocalImageInfo
                        {
                            FileInfo = _directoryService.GetFile(imagePng),
                            Type = image.Type
                        });
                        continue;
                    }
                    var url = image.Url;
                    await Retry.InvokeAsync(async () =>
                    {
                        var response = await item.RemoteImageProvider.GetImageResponse(image.Url, token);
                        // Sometimes providers send back bad urls. Just move to the next image
                        if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            _logger.LogInformation("{Url} returned {StatusCode}, ignoring", url, response.StatusCode);
                            return;
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("{Url} returned {StatusCode}, skipping all remaining requests", url, response.StatusCode);
                            throw new Exception();
                        }

                        var source = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
                        var mimeType = response.Content.Headers.ContentType?.MediaType;
                        var extension = MimeTypes.ToExtension(mimeType);
                        var path = Path.Combine(_serverApplicationPaths.ImageCachePath, imageHash + extension);
                        Directory.CreateDirectory(_serverApplicationPaths.ImageCachePath);
                        var fileStreamOptions = AsyncFile.WriteOptions;
                        fileStreamOptions.Mode = FileMode.Create;
                        fileStreamOptions.PreallocationSize = source.Length;
                        var fs = new FileStream(path, fileStreamOptions);
                        await using (fs.ConfigureAwait(false))
                        {
                            await source.CopyToAsync(fs, token).ConfigureAwait(false);
                        }

                        imagsList.Add(new LocalImageInfo
                        {
                            FileInfo = _directoryService.GetFile(path),
                            Type = image.Type
                        });
                    }, 3, 1000, false);
                }
            }

            return item;
        }

        private MoveMetaDataItem MergeMetaData(MoveMetaDataItem item)
        {
            if (!item.HasMetaData)
            {
                return item;
            }

            item.Result = item.MetadataResult.Item;
            if (item.LocalImageInfos?.Any() ?? false)
            {
                foreach (var imageInfo in item.LocalImageInfos)
                {
                    var itemImageInfo = new ItemImageInfo
                    {
                        Path = imageInfo.FileInfo.FullName,
                        Type = imageInfo.Type
                    };
                    item.Result.AddImage(itemImageInfo);
                }
            }
            if (item.LocalRemoteImageInfos?.Any() ?? false)
            {
                foreach (var imageInfo in item.LocalRemoteImageInfos)
                {
                    var itemImageInfo = new ItemImageInfo
                    {
                        Path = imageInfo.FileInfo.FullName,
                        Type = imageInfo.Type
                    };
                    item.Result.AddImage(itemImageInfo);
                }
            }
            return item;
        }
    }
}
