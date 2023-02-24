// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    internal static class Http
    {
        private static readonly Lazy<HttpClientHandler> HttpClientHandlerLazy = new(() =>
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false,
                AllowAutoRedirect = false
            };

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression =
                            DecompressionMethods.GZip |
                            DecompressionMethods.Deflate;
            }

            return handler;
        });

        public static HttpClientHandler ClientHandler => HttpClientHandlerLazy.Value;

        private static readonly Dictionary<string, Lazy<HttpClientHandler>>
            HttpClientHandlerLazyDict = new();

        public static HttpClientHandler GetClientHandler(string proxy)
        {
            if (HttpClientHandlerLazyDict.TryGetValue(proxy, out var client))
            {
                return client.Value;
            }

            HttpClientHandlerLazyDict.Add(proxy, new(() =>
            {
                var webProxy = null as IWebProxy;

                try
                {
                    if (proxy.StartsWith("socks5"))
                    {
                        var hostnamePort = proxy.Split(new char[] { '/' },
                            StringSplitOptions.RemoveEmptyEntries).Last();
                        var split = hostnamePort.Split(new char[] { ':' }, 2,
                            StringSplitOptions.RemoveEmptyEntries);
                        var hostname = split[0];
                        var ip = int.Parse(split[1]);
                        webProxy = new HttpToSocks5Proxy(hostname, ip);
                    }
                    else
                    {
                        webProxy = new WebProxy(proxy)
                        {
                            BypassProxyOnLocal = false
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Proxy url error.", ex);
                }

                var handler = new HttpClientHandler
                {
                    UseCookies = false,
                    AllowAutoRedirect = false,
                    Proxy = webProxy
                };

                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression =
                                    DecompressionMethods.GZip |
                                    DecompressionMethods.Deflate;
                }

                return handler;
            }));
            return HttpClientHandlerLazyDict[proxy].Value;
        }
    }
}
