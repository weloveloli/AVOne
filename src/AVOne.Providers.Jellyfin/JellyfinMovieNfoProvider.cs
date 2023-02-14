// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers.Jellyfin
{
    using AVOne.Configuration;
    using AVOne.Providers.Jellyfin.Base;
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
