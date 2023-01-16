// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers
{
    using AVOne.Abstraction;
    using AVOne.Models.Info;

    public interface INameResolverProvider : IProvider
    {
        VideoFileInfo ResolveVideo(string path, bool directory);
    }
}
