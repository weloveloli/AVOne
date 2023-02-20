// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Models
{
    using MediaBrowser.Controller.Entities.Movies;
    using MediaBrowser.Controller.Providers;
    using MediaBrowser.Model.Providers;

    public class MoveMetaDataItem
    {
        public Movie Movie { get; set; }

        public ILocalMetadataProvider<Movie> LocalMetadataProvider { get; set; }
        public IRemoteMetadataProvider<Movie, MovieInfo> RemoteMetadataProvider { get; set; }

        public ILocalImageProvider LocalImageProvider { get; set; }
        public IRemoteImageProvider RemoteImageProvider { get; set; }

        public MetadataResult<Movie> MetadataResult { get; set; }

        public IEnumerable<LocalImageInfo> LocalImageInfos { get; set; }
        public IEnumerable<RemoteImageInfo> RemoteImageInfos { get; set; }

        public IEnumerable<LocalImageInfo> LocalRemoteImageInfos { get; set; }

        public bool HasMetaData { get; set; } = false;

        public Movie Result { get; set; }

        public string Name => HasMetaData ? Result.Name : Movie.Name;

    }
}
