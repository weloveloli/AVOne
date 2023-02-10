// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Jellyfin
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Impl.Providers.Jellyfin.Base;
    using AVOne.IO;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Nfo saver for movies.
    /// </summary>
    public class JellyfinMovieNfoSaver : BaseJellifinNfoSaver
    {
        public override int Order => -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="JellyfinMovieNfoSaver"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="configurationManager">the server configuration manager.</param>
        /// <param name="libraryManager">The library manager.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="userDataManager">The user data manager.</param>
        /// <param name="logger">The logger.</param>
        public JellyfinMovieNfoSaver(
            IFileSystem fileSystem,
            IConfigurationManager configurationManager,
            ILogger<JellyfinMovieNfoSaver> logger)
            : base(fileSystem, configurationManager, logger)
        {
        }

        /// <inheritdoc />
        protected override string GetLocalSavePath(BaseItem item)
            => GetMovieSavePaths(new ItemInfo(item)).FirstOrDefault() ?? Path.ChangeExtension(item.Path, ".nfo");

        internal static IEnumerable<string> GetMovieSavePaths(ItemInfo item)
        {
            if (item.VideoType == VideoType.Dvd && !item.IsPlaceHolder)
            {
                var path = item.ContainingFolderPath;

                yield return Path.Combine(path, "VIDEO_TS", "VIDEO_TS.nfo");
            }

            if (!item.IsPlaceHolder && (item.VideoType == VideoType.Dvd || item.VideoType == VideoType.BluRay))
            {
                var path = item.ContainingFolderPath;

                yield return Path.Combine(path, Path.GetFileName(path) + ".nfo");
            }
            else
            {
                yield return Path.ChangeExtension(item.Path, ".nfo");

                if (!item.IsInMixedFolder)
                {
                    yield return Path.Combine(item.ContainingFolderPath, "movie.nfo");
                }
            }
        }

        /// <inheritdoc />
        protected override string GetRootElementName(BaseItem item)
            => "movie";

        /// <inheritdoc />
        public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            if (!item.SupportsLocalMetadata)
            {
                return false;
            }

            // Check parent for null to avoid running this against things like video backdrops
            return item is Video && updateType >= ItemUpdateType.ImageUpdate;
        }

        /// <inheritdoc />
        protected override void WriteCustomElements(BaseItem item, XmlWriter writer)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetTagsUsed(BaseItem item)
        {
            foreach (var tag in base.GetTagsUsed(item))
            {
                yield return tag;
            }

            yield return "album";
            yield return "artist";
            yield return "set";
            yield return "id";
        }
    }
}
