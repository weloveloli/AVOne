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
        /// <summary>
        /// Occurs when [item added].
        /// </summary>
        public event EventHandler<StatusChangeArgs> StatusChanged;

        public PornMovie Source { get; set; }

        public ILocalMetadataProvider<PornMovie> LocalMetadataProvider { get; set; }
        public IRemoteMetadataProvider<PornMovie, PornMovieInfo> RemoteMetadataProvider { get; set; }

        public ILocalImageProvider LocalImageProvider { get; set; }
        public IRemoteImageProvider RemoteImageProvider { get; set; }

        public IMetadataFileSaverProvider MetadataFileSaverProvider { get; set; }
        public IImageSaverProvider ImageSaverProvider { get; set; }

        public MetadataResult<PornMovie> MetadataResult { get; set; }

        public IEnumerable<LocalImageInfo> LocalImageInfos { get; set; }
        public IEnumerable<RemoteImageInfo> RemoteImageInfos { get; set; }

        public IEnumerable<LocalImageInfo> LocalRemoteImageInfos { get; set; }

        public bool HasMetaData { get; set; } = false;

        public PornMovie MovieWithMetaData { get; set; }

        public string Name => HasMetaData ? MovieWithMetaData.Name : Source.Name;

        public void UpdateStatus(string message, params object[] args) => StatusChanged?.Invoke(this, new StatusChangeArgs { StatusMessage = string.Format(message, args) });

    }
}
