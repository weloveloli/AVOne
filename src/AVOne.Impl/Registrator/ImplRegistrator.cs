// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Registrator
{
    using AVOne.Abstraction;
    using AVOne.IO;
    using AVOne.Models.Item;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal class ImplRegistrator : IServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
        }

        public void RegisterStatic(IApplicationHost host)
        {
            BaseItem.FileSystem = host.Resolve<IFileSystem>();
            BaseItem.Logger = host.Resolve<ILogger<BaseItem>>();
        }
    }
}
