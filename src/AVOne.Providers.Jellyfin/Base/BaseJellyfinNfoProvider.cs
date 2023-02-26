// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Jellyfin.Base
{
    using AVOne.IO;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Models.Result;
    using AVOne.Providers.Metadata;

    public abstract class BaseJellyfinNfoProvider<T> : ILocalMetadataProvider<T>
        where T : BaseItem, new()
    {
        private readonly IFileSystem _fileSystem;

        protected BaseJellyfinNfoProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <inheritdoc />
        public string Name => "Jellyfin.Nfo";

        /// <inheritdoc />
        public Task<MetadataResult<T>> GetMetadata(
            ItemInfo info,
            IDirectoryService directoryService,
            CancellationToken cancellationToken)
        {
            var result = new MetadataResult<T>();

            var file = GetXmlFile(info, directoryService);

            if (file == null)
            {
                return Task.FromResult(result);
            }

            var path = file.FullName;

            try
            {
                result.Item = new T();

                Fetch(result, path, cancellationToken);
                result.HasMetadata = true;
            }
            catch (FileNotFoundException)
            {
                result.HasMetadata = false;
            }
            catch (IOException)
            {
                result.HasMetadata = false;
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public bool HasChanged(BaseItem item, IDirectoryService directoryService)
        {
            var file = GetXmlFile(new ItemInfo(item), directoryService);

            return file != null && file.Exists && _fileSystem.GetLastWriteTimeUtc(file) > item.DateLastSaved;
        }

        protected abstract void Fetch(MetadataResult<T> result, string path, CancellationToken cancellationToken);

        protected abstract FileSystemMetadata? GetXmlFile(ItemInfo info, IDirectoryService directoryService);
    }
}
