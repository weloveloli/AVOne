// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable
namespace AVOne.Impl.Resolvers
{
    using System.Collections.Generic;
    using AVOne.Constants;
    using AVOne.Extensions;
    using AVOne.Enum;
    using AVOne.IO;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using AVOne.Resolvers;
    using Microsoft.Extensions.Logging;
    using System.Text.RegularExpressions;
    using AVOne.Models.Info;

    public class MovieResolver : BaseVideoResolver<Video>, IMultiItemResolver
    {
        private string[] _validCollectionTypes = new[]
{
                CollectionType.PronMovies,
                CollectionType.HomeVideos
        };

        /// <summary>
        /// Gets the priority.
        /// </summary>
        /// <value>The priority.</value>
        public override ResolverPriority Priority => ResolverPriority.Fourth;

        public MovieResolver(ILogger<MovieResolver> logger, IProviderManager provider) : base(logger, provider)
        {
        }

        /// <summary>
        /// Resolves the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>Video.</returns>
        protected override Video Resolve(ItemResolveArgs args)
        {
            var collectionType = args.GetCollectionType();

            // Find movies with their own folders
            if (args.IsDirectory)
            {
                if (IsInvalid(args.Parent, collectionType))
                {
                    return null;
                }

                Video movie = null;
                var files = args.GetActualFileSystemChildren().ToList();

                if (string.IsNullOrEmpty(collectionType))
                {
                    // Owned items will be caught by the video extra resolver
                    if (args.Parent == null)
                    {
                        return null;
                    }

                    movie = FindMovie<PornMovie>(args, args.Path, args.Parent, files, args.DirectoryService, collectionType, true);
                }

                // ignore extras
                return movie?.ExtraType == null ? movie : null;
            }

            if (args.Parent == null)
            {
                return base.Resolve(args);
            }

            if (IsInvalid(args.Parent, collectionType))
            {
                return null;
            }

            Video item = null;

            // To find a movie file, the collection type must be movies or boxsets
            if (string.Equals(collectionType, CollectionType.PronMovies, StringComparison.OrdinalIgnoreCase))
            {
                item = ResolveVideo<PornMovie>(args, true);
            }
            else if (string.Equals(collectionType, CollectionType.HomeVideos, StringComparison.OrdinalIgnoreCase))
            {
                item = ResolveVideo<Video>(args, false);
            }
            else if (string.IsNullOrEmpty(collectionType))
            {
                item = ResolveVideo<Video>(args, false);
            }

            // Ignore extras
            if (item?.ExtraType != null)
            {
                return null;
            }

            return item;
        }

        public MultiItemResolverResult ResolveMultiple(Folder parent, List<FileSystemMetadata> files, string collectionType, IDirectoryService directoryService)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds a movie based on a child file system entries.
        /// </summary>
        /// <returns>Movie.</returns>
        private T FindMovie<T>(ItemResolveArgs args, string path, Folder parent, List<FileSystemMetadata> fileSystemEntries, IDirectoryService directoryService, string collectionType, bool parseName)
            where T : Video, new()
        {
            var multiDiscFolders = new List<FileSystemMetadata>();
            var supportPhotos = false;
            var photos = new List<FileSystemMetadata>();

            // Search for a folder rip
            foreach (var child in fileSystemEntries)
            {
                var filename = child.Name;

                if (child.IsDirectory)
                {
                    if (IsDvdDirectory(child.FullName, filename, directoryService))
                    {
                        var movie = new T
                        {
                            Path = path,
                            VideoType = VideoType.Dvd
                        };
                        return movie;
                    }

                    if (IsBluRayDirectory(filename))
                    {
                        var movie = new T
                        {
                            Path = path,
                            VideoType = VideoType.BluRay
                        };
                        return movie;
                    }

                    multiDiscFolders.Add(child);
                }
                else if (IsDvdFile(filename))
                {
                    var movie = new T
                    {
                        Path = path,
                        VideoType = VideoType.Dvd
                    };
                    return movie;
                }
                else if (supportPhotos)
                {
                    photos.Add(child);
                }
            }

            // TODO: Allow GetMultiDiscMovie in here
            const bool SupportsMultiVersion = true;

            var result = ResolveVideos<T>(parent, fileSystemEntries, SupportsMultiVersion, collectionType, parseName) ??
                new MultiItemResolverResult();

            if (result.Items.Count == 1)
            {
                var videoPath = result.Items[0].Path;
                var hasPhotos = false;

                if (!hasPhotos)
                {
                    var movie = (T)result.Items[0];
                    movie.Name = Path.GetFileName(movie.ContainingFolderPath);
                    return movie;
                }
            }
            else if (result.Items.Count == 0 && multiDiscFolders.Count > 0)
            {
                return null;
            }

            return null;
        }

        private bool IsInvalid(Folder parent, ReadOnlySpan<char> collectionType)
        {
            if (parent != null)
            {
                if (parent.IsRoot)
                {
                    return true;
                }
            }

            if (collectionType.IsEmpty)
            {
                return false;
            }

            return !_validCollectionTypes.Contains(collectionType, StringComparison.OrdinalIgnoreCase);
        }
        private MultiItemResolverResult ResolveVideos<T>(
            Folder parent,
            IEnumerable<FileSystemMetadata> fileSystemEntries,
            bool supportMultiEditions,
            string collectionType,
            bool parseName)
            where T : Video, new()
        {
            var files = new List<FileSystemMetadata>();
            var leftOver = new List<FileSystemMetadata>();
            var hasCollectionType = !string.IsNullOrEmpty(collectionType);

            // Loop through each child file/folder and see if we find a video
            foreach (var child in fileSystemEntries)
            {
                // This is a hack but currently no better way to resolve a sometimes ambiguous situation
                if (!hasCollectionType)
                {
                    if (string.Equals(child.Name, "tvshow.nfo", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(child.Name, "season.nfo", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }

                if (child.IsDirectory)
                {
                    leftOver.Add(child);
                }
                else if (!IsIgnored(child.Name))
                {
                    files.Add(child);
                }
            }

            var videoInfos = files
                .Select(i => this.VideoResolver.Resolve(i.FullName, i.IsDirectory, NamingOptions, parseName))
                .Where(f => f != null)
                .ToList();

            var resolverResult = VideoListResolver.Resolve(this.VideoResolver, videoInfos, NamingOptions, supportMultiEditions, parseName);

            var result = new MultiItemResolverResult
            {
                ExtraFiles = leftOver
            };

            var isInMixedFolder = resolverResult.Count > 1 || parent?.IsTopParent == true;

            foreach (var video in resolverResult)
            {
                var firstVideo = video.Files[0];
                var path = firstVideo.Path;
                if (video.ExtraType != null)
                {
                    result.ExtraFiles.Add(files.Find(f => string.Equals(f.FullName, path, StringComparison.OrdinalIgnoreCase)));
                    continue;
                }

                var additionalParts = video.Files.Count > 1 ? video.Files.Skip(1).Select(i => i.Path).ToArray() : Array.Empty<string>();

                var videoItem = new T
                {
                    Path = path,
                    IsInMixedFolder = isInMixedFolder,
                    ProductionYear = video.Year,
                    Name = parseName ? video.Name : firstVideo.Name,
                    AdditionalParts = additionalParts,
                    LocalAlternateVersions = video.AlternateVersions.Select(i => i.Path).ToArray()
                };

                SetVideoType(videoItem, firstVideo);

                result.Items.Add(videoItem);
            }

            result.ExtraFiles.AddRange(files.Where(i => !ContainsFile(resolverResult, i)));

            return result;
        }
        private static bool IsIgnored(string filename)
        {
            // Ignore samples
            Match m = Regex.Match(filename, @"\bsample\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            return m.Success;
        }

        private static bool ContainsFile(IReadOnlyList<VideoInfo> result, FileSystemMetadata file)
        {
            for (var i = 0; i < result.Count; i++)
            {
                var current = result[i];
                for (var j = 0; j < current.Files.Count; j++)
                {
                    if (ContainsFile(current.Files[j], file))
                    {
                        return true;
                    }
                }

                for (var j = 0; j < current.AlternateVersions.Count; j++)
                {
                    if (ContainsFile(current.AlternateVersions[j], file))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ContainsFile(VideoFileInfo result, FileSystemMetadata file)
        {
            return string.Equals(result.Path, file.FullName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
