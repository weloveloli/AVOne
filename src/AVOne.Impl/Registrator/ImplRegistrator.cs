// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Registrator
{
    using AVOne.Abstraction;
    using AVOne.Configuration;
    using AVOne.Impl.Data;
    using AVOne.Impl.IO;
    using AVOne.Impl.Library;
    using AVOne.Impl.Providers;
    using AVOne.Impl.Updates;
    using AVOne.IO;
    using AVOne.Library;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using AVOne.Providers.Download;
    using AVOne.Providers.Extractor;
    using AVOne.Providers.Metadata;
    using AVOne.Resolvers;
    using AVOne.Updates;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ImplRegistrator : IServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IProviderManager, ProviderManager>();
            serviceCollection.AddSingleton<ILibraryManager, LibraryManager>();
            serviceCollection.AddSingleton<IInstallationManager, InstallationManager>();
            serviceCollection.AddSingleton<IFileSystem, ManagedFileSystem>();
            serviceCollection.AddSingleton<IDirectoryService, DirectoryService>();
            serviceCollection.AddSingleton<ApplicationDbContext>(sp =>
            {
                return ApplicationDbContext.Create(sp.GetService<IApplicationPaths>());
            })
                .AddSingleton<JobRepository>();
        }

        public void PostBuildService(IApplicationHost host)
        {
            BaseItem.FileSystem = host.Resolve<IFileSystem>();
            BaseItem.Logger = host.Resolve<ILogger<BaseItem>>();
            BaseItem.ConfigurationManager = host.Resolve<IConfigurationManager>();

            host.Resolve<IProviderManager>().AddParts(
                host.GetExports<IImageProvider>(),
                host.GetExports<IMetadataProvider>(),
                host.GetExports<INamingOptionProvider>(),
                host.GetExports<IVideoResolverProvider>(),
                host.GetExports<IMetadataSaverProvider>(),
                host.GetExports<IImageSaverProvider>(),
                host.GetExports<IDownloaderProvider>(),
                host.GetExports<IMediaExtractorProvider>()
                );

            host.Resolve<ILibraryManager>().AddParts(
                host.GetExports<IResolverIgnoreRule>(),
                host.GetExports<IItemResolver>());

            host.Resolve<IConfigurationManager>()
                .AddParts(host.GetExports<IConfigurationFactory>());
        }
    }
}
