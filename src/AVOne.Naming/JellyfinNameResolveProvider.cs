// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Naming
{
    using AVOne.Enum;
    using AVOne.Providers;
    using Emby.Naming.Common;
    using Emby.Naming.Video;
    using VideoFileInfo = AVOne.Models.Info.VideoFileInfo;

    public class JellyfinNameResolveProvider : INameResolverProvider
    {
        public const string Jellyfin = "Jellyfin";

        private readonly NamingOptions nameOptions;

        public string Name => Jellyfin;

        public JellyfinNameResolveProvider()
        {
            this.nameOptions = new NamingOptions();
        }

        public VideoFileInfo? ResolveVideo(string path, bool directory)
        {
            return CastToFileInfo(VideoResolver.Resolve(path, directory, this.nameOptions));
        }

        public static VideoFileInfo? CastToFileInfo(Emby.Naming.Video.VideoFileInfo? info)
        {
            return info is not null ? new VideoFileInfo(info.Name, info.Path, (ExtraType?)info.ExtraType, info.IsDirectory) : null;
        }
    }
}
