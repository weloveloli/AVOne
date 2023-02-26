// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Jellyfin
{
    using AVOne.Configuration;
    using AVOne.IO;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using AVOne.Providers.Jellyfin.Base;
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
