// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable
namespace AVOne.Tool.Facade
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Helper;
    using AVOne.Configuration;
    using AVOne.Constants;
    using AVOne.Enum;
    using AVOne.Impl.Configuration;
    using AVOne.IO;
    using AVOne.Library;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using AVOne.Providers.Metadata;
    using AVOne.Tool.Models;
    using Furion.FriendlyException;
    using Microsoft.Extensions.Logging;

    public class MetaDataFacade : IMetaDataFacade
    {
        private readonly ILogger<MetaDataFacade> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IProviderManager _providerManager;
        private readonly IDirectoryService _directoryService;
        private readonly IConfigurationManager _configurationManager;
        private readonly IApplicationPaths _serverApplicationPaths;

        private ProviderConfig _config => _configurationManager.CommonConfiguration.ProviderConfig;

        public MetaDataFacade(
            ILogger<MetaDataFacade> logger,
            ILibraryManager libraryManager,
            IProviderManager providerManager,
            IDirectoryService directoryService,
            IConfigurationManager configurationManager,
            IApplicationPaths serverApplicationPaths
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
                CollectionType.PornMovies);
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
                CollectionType.PornMovies);
            return GetMovie(items, token);
        }

        private async Task<IEnumerable<MoveMetaDataItem>> GetMovie(IEnumerable<BaseItem> items, CancellationToken token = default)
        {
            if (items == null || !items.Any())
            {
                return Enumerable.Empty<MoveMetaDataItem>();
            }

            var movis = items.OfType<PornMovie>().Select(movieItem =>
            {
                var m = new MoveMetaDataItem { Movie = movieItem };
                var providers = _providerManager
                    .GetMetadataProviders<PornMovie>(movieItem)
                    .Where(e => _config.ScanMetaDataProviders.Contains(e.Name));

                m.LocalMetadataProvider = providers
                    .OfType<ILocalMetadataProvider<PornMovie>>()
                    .FirstOrDefault();

                m.RemoteMetadataProvider = providers
                    .OfType<IRemoteMetadataProvider<PornMovie, PornMovieInfo>>()
                    .FirstOrDefault();
                var imageProviders = _providerManager.GetImageProviders(movieItem);

                m.LocalImageProvider = imageProviders
                    .OfType<ILocalImageProvider>()
                    .FirstOrDefault();

                m.RemoteImageProvider = imageProviders
                    .OfType<IRemoteImageProvider>()
                    .Where(e => _config.ImageMetaDataProviders.Contains(e.Name))
                    .FirstOrDefault();

                m.ImageSaverProvider = _providerManager.GetImageSaverProvider().FirstOrDefault();

                m.MetadataFileSaverProvider = _providerManager
                .GetMetadataSaverProvider(movieItem, ItemUpdateType.MetadataDownload)
                .OfType<IMetadataFileSaverProvider>()
                .FirstOrDefault();
                return m;
            });

            var tasks = movis.Select(e => ExecuteMetaData(e, token));

            var movieItems = await Task.WhenAll(tasks);

            return movieItems.Select(MergeMetaData);
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
                RemoteMetadataProvider = dataItem.RemoteMetadataProvider,
                ImageSaverProvider = dataItem.ImageSaverProvider,
                MetadataFileSaverProvider = dataItem.MetadataFileSaverProvider
            };

            var metaDataResult = await item.LocalMetadataProvider
                .GetMetadata(new ItemInfo(item.Movie), _directoryService, token);
            if (metaDataResult.HasMetadata)
            {
                item.HasMetaData = true;
                item.MetadataResult = metaDataResult;
                item.Result = metaDataResult.Item;
                item.Result.Path = item.Movie.Path;
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
                        item.Result.Path = item.Movie.Path;
                        item.Result.People?.Clear();
                        item.Result.People.AddRange(metaDataResult.People);
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
            if (!item.LocalImageInfos.Any())
            {
                item.RemoteImageInfos = await item.RemoteImageProvider.GetImages(item.Result, token);

                if (item.RemoteImageInfos != null && item.RemoteImageInfos.Any())
                {
                    var imagsList = new List<LocalImageInfo>();
                    item.LocalRemoteImageInfos = imagsList;
                    foreach (var image in item.RemoteImageInfos)
                    {
                        if (image.Type != ImageType.Primary)
                        {
                            continue;
                        }
                        var imageCachePath = await DownloadRemoteImageToCache(item.RemoteImageProvider, image, token);
                        imagsList.Add(new LocalImageInfo
                        {
                            FileInfo = _directoryService.GetFile(imageCachePath),
                            Type = image.Type
                        });
                    }
                }
            }

            return item;
        }

        private async Task<string> DownloadRemoteImageToCache(IRemoteImageProvider remoteImageProvider, RemoteImageInfo image, CancellationToken token = default)
        {
            var hash = SHA256.Create();
            var imageHash = string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(image.Url)).Select(x => x.ToString("x2")));

            var imagePng = Path.Combine(_serverApplicationPaths.ImageCachePath, imageHash + ".png");
            var imageJpg = Path.Combine(_serverApplicationPaths.ImageCachePath, imageHash + ".jpg");
            string cacheFile = null;
            if (File.Exists(imageJpg))
            {
                cacheFile = imageJpg;
            }
            else if (File.Exists(imagePng))
            {
                cacheFile = imagePng;
            }
            if (cacheFile is not null)
            {
                return cacheFile;
            }

            var url = image.Url;
            string path = null;
            await Retry.InvokeAsync(async () =>
            {
                var response = await remoteImageProvider.GetImageResponse(image.Url, token);
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

                using var source = response.Content.ReadAsStream();
                var mimeType = response.Content.Headers.ContentType?.MediaType;
                var extension = MimeTypesHelper.ToExtension(mimeType);
                path = Path.Combine(_serverApplicationPaths.ImageCachePath, imageHash + extension);
                Directory.CreateDirectory(_serverApplicationPaths.ImageCachePath);

                var fileStreamOptions = FileOptionsHelper.AsyncWriteOptions;
                fileStreamOptions.Mode = FileMode.Create;
                fileStreamOptions.PreallocationSize = source.Length;
                var fs = new FileStream(path, fileStreamOptions);
                await using (fs.ConfigureAwait(false))
                {
                    await source.CopyToAsync(fs, token).ConfigureAwait(false);
                }
            }, 3, 1000, false);
            return path;
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

        public async Task SaveMetaDataToLocal(MoveMetaDataItem item, CancellationToken token = default)
        {
            if (item.LocalImageInfos != null && item.LocalImageInfos.Any())
            {
                foreach (var image in item.LocalImageInfos)
                {
                    var oldPath = image.FileInfo.FullName;
                    var newPath = Path.Combine(Directory.GetParent(item.Result.TargetPath).FullName, image.FileInfo.Name);
                    if (oldPath != newPath)
                    {
                        File.Move(oldPath, newPath);
                    }
                }
            }
            else if (item.RemoteImageInfos != null && item.RemoteImageInfos.Any())
            {
                item.UpdateStatus(L.Text["Downloading Remote Image"], item.Movie.Name);
                foreach (var image in item.RemoteImageInfos)
                {
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

                        using var source = response.Content.ReadAsStream();
                        var mimeType = response.Content.Headers.ContentType?.MediaType;
                        await item.ImageSaverProvider.SaveImage(item.Result, source, mimeType, image.Type, null, token);
                    }, 3, 1000, false);
                }
            }
            item.UpdateStatus(L.Text["Saving metadata"], item.Movie.Name);
            await item.MetadataFileSaverProvider.SaveAsync(item.Result, token);
        }
    }
}
