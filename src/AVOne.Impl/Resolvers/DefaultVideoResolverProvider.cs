// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Resolvers
{
    using AVOne.Models.Info;
    using AVOne.Providers.Metadata;

    public class DefaultVideoResolverProvider : IVideoResolverProvider
    {
        public int Order => int.MaxValue;

        public string Name => "Jellyfin";

        public bool IsVideoFile(string path, INamingOptions namingOptions)
        {
            return VideoResolver.IsVideoFile(path, namingOptions);
        }

        public VideoFileInfo? Resolve(string path, bool v, INamingOptions namingOptions, bool parseName)
        {
            return VideoResolver.Resolve(path, v, namingOptions, parseName);
        }

        public VideoFileInfo? ResolveDirectory(string path, INamingOptions namingOptions, bool parseName)
        {
            return VideoResolver.ResolveDirectory(path, namingOptions, parseName);
        }
    }
}
