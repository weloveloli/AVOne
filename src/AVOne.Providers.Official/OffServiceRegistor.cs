// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official
{
    using System.Net.Http.Headers;
    using AVOne.Abstraction;
    using AVOne.Constants;
    using Microsoft.Extensions.DependencyInjection;

    internal class OffServiceRegistrator : IServiceRegistrator
    {
        public void PostBuildService(IApplicationHost host)
        {
        }

        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient(Constants.Official, (client) =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36 Edg/94.0.992.50");
                client.DefaultRequestHeaders.AcceptLanguage.Clear();
                client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh-CN"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

            }).ConfigurePrimaryHttpMessageHandler((sp) =>
            {
                var handler = new HttpClientHandler();
                handler.AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip;
                return handler;
            });

            serviceCollection.AddHttpClient(AVOneConstants.Download, (client) =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36 Edg/94.0.992.50");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
            }).ConfigurePrimaryHttpMessageHandler((builder) =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (m, c, ch, e) => { return true; }
                };
                return handler;

            })
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
                    PooledConnectionIdleTimeout = TimeSpan.FromSeconds(90),
                });

        }
    }
}
