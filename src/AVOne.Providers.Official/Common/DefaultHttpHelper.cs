// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Common
{
    using System.Net;
    using AVOne.Common.Helper;
    using AVOne.Configuration;

    public class DefaultHttpHelper : HttpClientHelper, IHttpHelper
    {
        public DefaultHttpHelper(
            IConfigurationManager manager,
            IHttpClientFactory httpClientFactory)
            : base(manager, httpClientFactory)
        {
        }

        public async Task<string> GetHtmlAsync(string url, CancellationToken token)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Version = HttpVersion.Version20
            };
            var resp = await GetHttpClient().SendAsync(req, token);
            resp.EnsureSuccessStatusCode();
            var html = await resp.Content.ReadAsStringAsync(token);
            return html;
        }
    }
}
