// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers.Jellyfin.Base
{
    using AVOne.Configuration;
    using AVOne.IO;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Models.Result;
    using AVOne.Providers;
    using AVOne.Providers.Jellyfin;
    using Microsoft.Extensions.Logging;

    public abstract class BaseVideoNfoProvider<T> : BaseNfoProvider<T>
        where T : Video, new()
    {
        private readonly ILogger<BaseVideoNfoProvider<T>> _logger;
        private readonly IConfigurationManager _config;
        private readonly IProviderManager _providerManager;
        private readonly IDirectoryService _directoryService;

        protected BaseVideoNfoProvider(
            ILogger<BaseVideoNfoProvider<T>> logger,
            IFileSystem fileSystem,
            IConfigurationManager config,
            IProviderManager providerManager,
            IDirectoryService directoryService)
            : base(fileSystem)
        {
            _logger = logger;
            _config = config;
            _providerManager = providerManager;
            _directoryService = directoryService;
        }

        /// <inheritdoc />
        protected override void Fetch(MetadataResult<T> result, string path, CancellationToken cancellationToken)
        {
            var tmpItem = new MetadataResult<T>
            {
                Item = result.Item
            };
            new BaseNfoParser<T>(_logger, _config, _providerManager, _directoryService).Fetch(tmpItem, path, cancellationToken);

            result.Item = tmpItem.Item;
            result.People = tmpItem.People;
            result.Images = tmpItem.Images;
            result.RemoteImages = tmpItem.RemoteImages;
        }

        /// <inheritdoc />
        protected override FileSystemMetadata? GetXmlFile(ItemInfo info, IDirectoryService directoryService)
        {

            return JellyfinMovieNfoSaver.GetMovieSavePaths(info)
                .Select(directoryService.GetFile)
                .FirstOrDefault(i => i != null);
        }
    }
}
