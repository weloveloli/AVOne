// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Registrator
{
    using AVOne.Abstraction;
    using AVOne.Impl.Library;
    using AVOne.Impl.Providers;
    using AVOne.IO;
    using AVOne.Library;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using AVOne.Resolvers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal class ImplRegistrator : IServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            _ = serviceCollection.AddSingleton<IProviderManager, ProviderManager>();
            _ = serviceCollection.AddSingleton<ILibraryManager, LibraryManager>();
        }

        public void PostBuildService(IApplicationHost host)
        {
            BaseItem.FileSystem = host.Resolve<IFileSystem>();
            BaseItem.Logger = host.Resolve<ILogger<BaseItem>>();
            host.Resolve<IProviderManager>().AddParts(
                host.GetExports<IImageProvider>(),
                host.GetExports<IMetadataProvider>(),
                host.GetExports<INamingOptionProvider>(),
                host.GetExports<IVideoResolverProvider>());

            host.Resolve<ILibraryManager>().AddParts(
                host.GetExports<IResolverIgnoreRule>(),
                host.GetExports<IItemResolver>());
        }
    }
}
