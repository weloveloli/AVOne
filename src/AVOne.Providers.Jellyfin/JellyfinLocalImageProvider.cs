// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Jellyfin
{
    using AVOne.Abstraction;
    using AVOne.Enum;
    using AVOne.Extensions;
    using AVOne.IO;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Providers.Metadata;

    /// <summary>
    /// Local image provider.
    /// </summary>
    public class JellyfinLocalImageProvider : ILocalImageProvider, IHasOrder
    {
        private static readonly string[] _commonImageFileNames =
        {
            "poster",
            "folder",
            "cover",
            "default"
        };

        private static readonly string[] _musicImageFileNames =
        {
            "folder",
            "poster",
            "cover",
            "default"
        };

        private static readonly string[] _personImageFileNames =
        {
            "folder",
            "poster"
        };

        private static readonly string[] _seriesImageFileNames =
        {
            "poster",
            "folder",
            "cover",
            "default",
            "show"
        };

        private static readonly string[] _videoImageFileNames =
        {
            "poster",
            "folder",
            "cover",
            "default",
            "movie"
        };

        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="JellyfinLocalImageProvider"/> class.
        /// </summary>
        /// <param name="fileSystem">Instance of the <see cref="IFileSystem"/> interface.</param>
        public JellyfinLocalImageProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <inheritdoc />
        public string Name => "Local Images";

        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        public bool Supports(BaseItem item)
        {
            if (item.SupportsLocalMetadata)
            {

                return true;
            }

            return false;
        }

        private static IEnumerable<FileSystemMetadata> GetFiles(BaseItem item, bool includeDirectories, IDirectoryService directoryService)
        {
            if (!item.IsFileProtocol)
            {
                return Enumerable.Empty<FileSystemMetadata>();
            }

            var path = item.ContainingFolderPath;

            // Exit if the cache dir does not exist, alternative solution is to create it, but that's a lot of empty dirs...
            if (!Directory.Exists(path))
            {
                return Enumerable.Empty<FileSystemMetadata>();
            }

            return directoryService.GetFileSystemEntries(path)
                .Where(i =>
                    includeDirectories && i.IsDirectory
                    || BaseItem.SupportedImageExtensions.Contains(i.Extension, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => Array.IndexOf(BaseItem.SupportedImageExtensions, i.Extension ?? string.Empty));
        }

        /// <inheritdoc />
        public IEnumerable<LocalImageInfo> GetImages(BaseItem item, IDirectoryService directoryService)
        {
            var files = GetFiles(item, true, directoryService).ToList();

            var list = new List<LocalImageInfo>();

            PopulateImages(item, list, files, true, directoryService);

            return list;
        }

        /// <summary>
        /// Get images for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="path">The images path.</param>
        /// <param name="directoryService">Instance of the <see cref="IDirectoryService"/> interface.</param>
        /// <returns>The local image info.</returns>
        public IEnumerable<LocalImageInfo> GetImages(BaseItem item, string path, IDirectoryService directoryService)
        {
            return GetImages(item, new[] { path }, directoryService);
        }

        /// <summary>
        /// Get images for item from multiple paths.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="paths">The image paths.</param>
        /// <param name="directoryService">Instance of the <see cref="IDirectoryService"/> interface.</param>
        /// <returns>The local image info.</returns>
        public IEnumerable<LocalImageInfo> GetImages(BaseItem item, IEnumerable<string> paths, IDirectoryService directoryService)
        {
            var files = paths.SelectMany(i => _fileSystem.GetFiles(i, BaseItem.SupportedImageExtensions, true, false));

            files = files
                .OrderBy(i => Array.IndexOf(BaseItem.SupportedImageExtensions, i.Extension ?? string.Empty));

            var list = new List<LocalImageInfo>();

            PopulateImages(item, list, files.ToList(), false, directoryService);

            return list;
        }

        private void PopulateImages(BaseItem item, List<LocalImageInfo> images, List<FileSystemMetadata> files, bool supportParentSeriesFiles, IDirectoryService directoryService)
        {
            if (supportParentSeriesFiles)
            {
            }

            var imagePrefix = item.FileNameWithoutExtension + "-";
            var isInMixedFolder = item.IsInMixedFolder;

            PopulatePrimaryImages(item, images, files, imagePrefix, isInMixedFolder);

            var added = false;
            var isEpisode = false;
            var isSong = false;
            var isPerson = item is Person;

            // Logo
            if (!isEpisode && !isSong && !isPerson)
            {
                added = AddImage(files, images, "logo", imagePrefix, isInMixedFolder, ImageType.Logo);
                if (!added)
                {
                    AddImage(files, images, "clearlogo", imagePrefix, isInMixedFolder, ImageType.Logo);
                }
            }

            // Art
            if (!isEpisode && !isSong && !isPerson)
            {
                AddImage(files, images, "clearart", imagePrefix, isInMixedFolder, ImageType.Art);
            }

            if (item is Video)
            {
                added = AddImage(files, images, "disc", imagePrefix, isInMixedFolder, ImageType.Disc);

                if (!added)
                {
                    added = AddImage(files, images, "cdart", imagePrefix, isInMixedFolder, ImageType.Disc);
                }

                if (!added)
                {
                    AddImage(files, images, "discart", imagePrefix, isInMixedFolder, ImageType.Disc);
                }
            }

            // Banner
            if (!isEpisode && !isSong && !isPerson)
            {
                AddImage(files, images, "banner", imagePrefix, isInMixedFolder, ImageType.Banner);
            }

            // Thumb
            if (!isEpisode && !isSong && !isPerson)
            {
                added = AddImage(files, images, "landscape", imagePrefix, isInMixedFolder, ImageType.Thumb);
                if (!added)
                {
                    AddImage(files, images, "thumb", imagePrefix, isInMixedFolder, ImageType.Thumb);
                }
            }

            if (!isEpisode && !isSong && !isPerson)
            {
                PopulateBackdrops(item, images, files, imagePrefix, isInMixedFolder);
            }
        }

        private void PopulatePrimaryImages(BaseItem item, List<LocalImageInfo> images, List<FileSystemMetadata> files, string imagePrefix, bool isInMixedFolder)
        {
            string[] imageFileNames;

            if (item is Person)
            {
                // these prefer folder
                imageFileNames = _personImageFileNames;
            }
            else if (item is Video)
            {
                imageFileNames = _videoImageFileNames;
            }
            else
            {
                imageFileNames = _commonImageFileNames;
            }

            var fileNameWithoutExtension = item.FileNameWithoutExtension;
            if (!string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                if (AddImage(files, images, fileNameWithoutExtension, ImageType.Primary))
                {
                    return;
                }
            }

            foreach (var name in imageFileNames)
            {
                if (AddImage(files, images, name, ImageType.Primary, imagePrefix))
                {
                    return;
                }
            }

            if (!isInMixedFolder)
            {
                foreach (var name in imageFileNames)
                {
                    if (AddImage(files, images, name, ImageType.Primary))
                    {
                        return;
                    }
                }
            }
        }

        private void PopulateBackdrops(BaseItem item, List<LocalImageInfo> images, List<FileSystemMetadata> files, string imagePrefix, bool isInMixedFolder)
        {
            if (!string.IsNullOrEmpty(item.Path))
            {
                var name = item.FileNameWithoutExtension;

                if (!string.IsNullOrEmpty(name))
                {
                    AddImage(files, images, name + "-fanart", ImageType.Backdrop, imagePrefix);

                    // Support without the prefix if it's in it's own folder
                    if (!isInMixedFolder)
                    {
                        AddImage(files, images, name + "-fanart", ImageType.Backdrop);
                    }
                }
            }

            PopulateBackdrops(images, files, imagePrefix, "fanart", "fanart-", isInMixedFolder, ImageType.Backdrop);
            PopulateBackdrops(images, files, imagePrefix, "background", "background-", isInMixedFolder, ImageType.Backdrop);
            PopulateBackdrops(images, files, imagePrefix, "art", "art-", isInMixedFolder, ImageType.Backdrop);

            var extraFanartFolder = files
                .FirstOrDefault(i => string.Equals(i.Name, "extrafanart", StringComparison.OrdinalIgnoreCase));

            if (extraFanartFolder is not null)
            {
                PopulateBackdropsFromExtraFanart(extraFanartFolder.FullName, images);
            }

            PopulateBackdrops(images, files, imagePrefix, "backdrop", "backdrop", isInMixedFolder, ImageType.Backdrop);
        }

        private void PopulateBackdropsFromExtraFanart(string path, List<LocalImageInfo> images)
        {
            var imageFiles = _fileSystem.GetFiles(path, BaseItem.SupportedImageExtensions, false, false);

            images.AddRange(imageFiles.Where(i => i.Length > 0).Select(i => new LocalImageInfo
            {
                FileInfo = i,
                Type = ImageType.Backdrop
            }));
        }

        private void PopulateBackdrops(List<LocalImageInfo> images, List<FileSystemMetadata> files, string imagePrefix, string firstFileName, string subsequentFileNamePrefix, bool isInMixedFolder, ImageType type)
        {
            AddImage(files, images, imagePrefix + firstFileName, type);

            var unfound = 0;
            for (var i = 1; i <= 20; i++)
            {
                // Screenshot Image
                var found = AddImage(files, images, imagePrefix + subsequentFileNamePrefix + i, type);

                if (!found)
                {
                    unfound++;

                    if (unfound >= 3)
                    {
                        break;
                    }
                }
            }

            // Support without the prefix
            if (!isInMixedFolder)
            {
                AddImage(files, images, firstFileName, type);

                unfound = 0;
                for (var i = 1; i <= 20; i++)
                {
                    // Screenshot Image
                    var found = AddImage(files, images, subsequentFileNamePrefix + i, type);

                    if (!found)
                    {
                        unfound++;

                        if (unfound >= 3)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private bool AddImage(List<FileSystemMetadata> files, List<LocalImageInfo> images, string name, string imagePrefix, bool isInMixedFolder, ImageType type)
        {
            var added = AddImage(files, images, name, type, imagePrefix);

            if (!isInMixedFolder)
            {
                if (AddImage(files, images, name, type))
                {
                    added = true;
                }
            }

            return added;
        }

        private static bool AddImage(IReadOnlyList<FileSystemMetadata> files, List<LocalImageInfo> images, string name, ImageType type, string? prefix = null)
        {
            var image = GetImage(files, name, prefix);

            if (image is null)
            {
                return false;
            }

            images.Add(new LocalImageInfo
            {
                FileInfo = image,
                Type = type
            });

            return true;
        }

        private static FileSystemMetadata? GetImage(IReadOnlyList<FileSystemMetadata> files, string name, string? prefix = null)
        {
            var fileNameLength = name.Length + (prefix?.Length ?? 0);
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file.IsDirectory || file.Length <= 0)
                {
                    continue;
                }

                var fileName = Path.GetFileNameWithoutExtension(file.FullName.AsSpan());
                if (fileName.Length == fileNameLength
                    && fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    && fileName.EndsWith(name, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }

            return null;
        }
    }
}
