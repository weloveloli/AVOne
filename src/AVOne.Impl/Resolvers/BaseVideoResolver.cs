// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

#pragma warning disable CS1591

namespace AVOne.Impl.Resolvers
{
    using AVOne.Enum;
    using AVOne.IO;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using AVOne.Providers.Metadata;
    using AVOne.Resolvers;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Resolves a Path into a Video or Video subclass.
    /// </summary>
    /// <typeparam name="T">The type of item to resolve.</typeparam>
    public abstract class BaseVideoResolver<T> : ItemResolver<T>
        where T : Video, new()
    {
        private readonly ILogger _logger;
        private readonly IProviderManager _provider;

        protected BaseVideoResolver(ILogger logger, IProviderManager provider)
        {
            _logger = logger;
            _provider = provider;
        }

        protected INamingOptions NamingOptions => _provider.GetNamingOptionProvider().GetNamingOption();
        protected IVideoResolverProvider VideoResolver => _provider.GetVideoResolverProvider();

        /// <summary>
        /// Resolves the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>`0.</returns>
        protected override T Resolve(ItemResolveArgs args)
        {
            return ResolveVideo<T>(args, false);
        }

        /// <summary>
        /// Resolves the video.
        /// </summary>
        /// <typeparam name="TVideoType">The type of the T video type.</typeparam>
        /// <param name="args">The args.</param>
        /// <param name="parseName">if set to <c>true</c> [parse name].</param>
        /// <returns>``0.</returns>
        protected virtual TVideoType ResolveVideo<TVideoType>(ItemResolveArgs args, bool parseName)
              where TVideoType : Video, new()
        {
            VideoFileInfo videoInfo = null;
            VideoType? videoType = null;

            // If the path is a file check for a matching extensions
            if (args.IsDirectory)
            {
                // Loop through each child file/folder and see if we find a video
                foreach (var child in args.FileSystemChildren)
                {
                    var filename = child.Name;
                    if (child.IsDirectory)
                    {
                        if (IsDvdDirectory(child.FullName, filename, args.DirectoryService))
                        {
                            videoType = VideoType.Dvd;
                        }
                        else if (IsBluRayDirectory(filename))
                        {
                            videoType = VideoType.BluRay;
                        }
                    }
                    else if (IsDvdFile(filename))
                    {
                        videoType = VideoType.Dvd;
                    }

                    if (videoType == null)
                    {
                        continue;
                    }

                    videoInfo = VideoResolver.ResolveDirectory(args.Path, NamingOptions, parseName);
                    break;
                }
            }
            else
            {
                videoInfo = VideoResolver.Resolve(args.Path, false, NamingOptions, parseName);
            }

            if (videoInfo == null || !VideoResolver.IsVideoFile(args.Path, NamingOptions))
            {
                return null;
            }

            var video = new TVideoType
            {
                Name = videoInfo.Name,
                Path = args.Path,
                ProductionYear = videoInfo.Year,
                ExtraType = videoInfo.ExtraType
            };

            if (videoType.HasValue)
            {
                video.VideoType = videoType.Value;
            }
            else
            {
                SetVideoType(video, videoInfo);
            }

            return video;
        }

        protected void SetVideoType(Video video, VideoFileInfo videoInfo)
        {
            var extension = Path.GetExtension(video.Path.AsSpan());
            video.VideoType = extension.Equals(".iso", StringComparison.OrdinalIgnoreCase)
                              || extension.Equals(".img", StringComparison.OrdinalIgnoreCase)
                ? VideoType.Iso
                : VideoType.VideoFile;
        }

        /// <summary>
        /// Determines whether [is DVD directory] [the specified directory name].
        /// </summary>
        /// <param name="fullPath">The full path of the directory.</param>
        /// <param name="directoryName">The name of the directory.</param>
        /// <param name="directoryService">The directory service.</param>
        /// <returns><c>true</c> if the provided directory is a DVD directory, <c>false</c> otherwise.</returns>
        protected bool IsDvdDirectory(string fullPath, string directoryName, IDirectoryService directoryService)
        {
            if (!string.Equals(directoryName, "video_ts", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return directoryService.GetFilePaths(fullPath).Any(i => string.Equals(Path.GetExtension(i), ".vob", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines whether [is DVD file] [the specified name].
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if [is DVD file] [the specified name]; otherwise, <c>false</c>.</returns>
        protected bool IsDvdFile(string name)
        {
            return string.Equals(name, "video_ts.ifo", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether [is bluray directory] [the specified directory name].
        /// </summary>
        /// <param name="directoryName">The directory name.</param>
        /// <returns>Whether the directory is a bluray directory.</returns>
        protected bool IsBluRayDirectory(string directoryName)
        {
            return string.Equals(directoryName, "bdmv", StringComparison.OrdinalIgnoreCase);
        }
    }
}
