// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Tool.Models
{
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Models.Result;
    using AVOne.Providers.Metadata;

    public class MoveMetaDataItem
    {
        public PornMovie Movie { get; set; }

        public ILocalMetadataProvider<PornMovie> LocalMetadataProvider { get; set; }
        public IRemoteMetadataProvider<PornMovie, PornMovieInfo> RemoteMetadataProvider { get; set; }

        public ILocalImageProvider LocalImageProvider { get; set; }
        public IRemoteImageProvider RemoteImageProvider { get; set; }

        public MetadataResult<PornMovie> MetadataResult { get; set; }

        public IEnumerable<LocalImageInfo> LocalImageInfos { get; set; }
        public IEnumerable<RemoteImageInfo> RemoteImageInfos { get; set; }

        public IEnumerable<LocalImageInfo> LocalRemoteImageInfos { get; set; }

        public bool HasMetaData { get; set; } = false;

        public PornMovie Result { get; set; }

        public string Name => HasMetaData ? Result.Name : Movie.Name;

    }
}
