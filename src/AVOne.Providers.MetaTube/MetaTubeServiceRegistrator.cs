// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.MetaTube
{
    using AVOne.Abstraction;
    using AVOne.Configuration;
    using AVOne.Providers.Metatube;
    using AVOne.Providers.MetaTube.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class MetaTubeServiceRegistrator : IServiceRegistrator
    {
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            _ = serviceCollection
                .AddHttpClient<MetatubeApiClient>()
                .ConfigureHttpMessageHandlerBuilder(sp => new SocketsHttpHandler
                {
                    // Connect Timeout.
                    ConnectTimeout = TimeSpan.FromSeconds(30),

                    // TCP Keep Alive.
                    KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(30),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),

                    // Connection Pooling.
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                    PooledConnectionIdleTimeout = TimeSpan.FromSeconds(90)
                });
            serviceCollection.AddSingleton<IConfigurationFactory, MetaTubeConfigurationFactory>();
        }

        public void PostBuildService(IApplicationHost host)
        {
        }
    }
}
