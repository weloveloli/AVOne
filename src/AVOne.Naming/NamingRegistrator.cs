// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Naming
{
    using AVOne.Abstraction;
    using AVOne.Providers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public class NamingRegistrator : IServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddProvider<INameResolverProvider, JellyfinNameResolveProvider>();
        }
    }
}
