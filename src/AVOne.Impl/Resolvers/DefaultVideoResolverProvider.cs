// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Resolvers
{
    using AVOne.Impl.Constants;
    using AVOne.Models.Info;
    using AVOne.Providers;

    public class DefaultVideoResolverProvider : IVideoResolverProvider
    {
        public string Name => OfficialProviderNames.Default;

        public int Order => int.MaxValue;

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
