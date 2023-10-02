// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Plugins.MetaTube
{
    using AVOne.Abstraction;
    using Microsoft.Extensions.DependencyInjection;

    public class MetaTubeServiceRegistrator : IServiceRegistrator
    {
        public void PostBuildService(IApplicationHost host)
        {
        }

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
        }
    }
}
