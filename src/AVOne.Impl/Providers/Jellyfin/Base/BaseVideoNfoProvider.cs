

namespace AVOne.Impl.Providers.Jellyfin.Base
{
    using AVOne.Configuration;
    using AVOne.IO;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Models.Result;
    using AVOne.Providers;
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
            var tmpItem = new MetadataResult<Video>
            {
                Item = result.Item
            };
            new MovieNfoParser(_logger, _config, _providerManager, _userManager, _userDataManager, _directoryService).Fetch(tmpItem, path, cancellationToken);

            result.Item = (T)tmpItem.Item;
            result.People = tmpItem.People;
            result.Images = tmpItem.Images;
            result.RemoteImages = tmpItem.RemoteImages;

            if (tmpItem.UserDataList != null)
            {
                result.UserDataList = tmpItem.UserDataList;
            }
        }

        /// <inheritdoc />
        protected override FileSystemMetadata? GetXmlFile(ItemInfo info, IDirectoryService directoryService)
        {
            return MovieNfoSaver.GetMovieSavePaths(info)
                .Select(directoryService.GetFile)
                .FirstOrDefault(i => i != null);
        }
    }
}
