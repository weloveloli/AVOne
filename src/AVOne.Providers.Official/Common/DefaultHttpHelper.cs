// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Common
{
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using AVOne.Common.Helper;
    using AVOne.Configuration;

    public class DefaultHttpHelper : HttpClientHelper, IHttpHelper
    {
        private string cachePath;

        public DefaultHttpHelper(
            IApplicationPaths applicationPaths,
            IConfigurationManager manager,
            IHttpClientFactory httpClientFactory)
            : base(manager, httpClientFactory)
        {
            this.cachePath = applicationPaths.ImageCachePath;
        }

        public async Task<string> DownloadThumbAsync(string url, Dictionary<string, string>? headers = null, CancellationToken token = default)
        {

            var req = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Version = HttpVersion.Version20
            };

            headers ??= new Dictionary<string, string>();

            foreach (var header in headers)
            {
                req.Headers.Add(header.Key, header.Value);
            }
            // download the image
            var resp = await GetHttpClient().SendAsync(req, token);
            resp.EnsureSuccessStatusCode();
            var stream = resp.Content.ReadAsStream(token);
            // use sha256 to generate the file name by the url
            var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(url));
            var fileName = Path.Combine(cachePath, BitConverter.ToString(hash).Replace("-", "") + ".jpg");
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                stream.CopyTo(fs);
            }
            return fileName;
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

        public Task<string> GetHtmlAsync(string url, Dictionary<string, string> headers, CancellationToken token = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Version = HttpVersion.Version20
            };

            foreach (var header in headers)
            {
                req.Headers.Add(header.Key, header.Value);
            }
            var resp = GetHttpClient().SendAsync(req, token);
            resp.Result.EnsureSuccessStatusCode();
            var html = resp.Result.Content.ReadAsStringAsync(token);
            return html;
        }
    }
}
