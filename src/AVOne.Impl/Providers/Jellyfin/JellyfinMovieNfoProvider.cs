// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Jellyfin
{
    using AVOne.Configuration;
    using AVOne.Impl.Providers.Jellyfin.Base;
    using AVOne.IO;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using Microsoft.Extensions.Logging;

    public class JellyfinMovieNfoProvider : BaseVideoNfoProvider<PornMovie>
    {
        public JellyfinMovieNfoProvider(ILogger<BaseVideoNfoProvider<PornMovie>> logger,
                                        IFileSystem fileSystem,
                                        IConfigurationManager config,
                                        IProviderManager providerManager,
                                        IDirectoryService directoryService) 
            : base(logger, fileSystem, config, providerManager, directoryService)
        {
        }
    }
}
