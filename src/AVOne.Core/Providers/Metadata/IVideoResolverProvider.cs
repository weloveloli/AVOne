// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable


namespace AVOne.Providers.Metadata
{
    using AVOne.Abstraction;
    using AVOne.Models.Info;

    public interface IVideoResolverProvider : IProvider, IHasOrder
    {
        bool IsVideoFile(string path, INamingOptions namingOptions);
        VideoFileInfo Resolve(string path, bool v, INamingOptions namingOptions, bool parseName);
        VideoFileInfo ResolveDirectory(string path, INamingOptions namingOptions, bool parseName);
    }
}
