// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Helper;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Providers.Extractor;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;

    public abstract class BaseHttpExtractor : HttpClientHelper, IMediaExtractorProvider
    {
        protected ILogger _logger;
        private readonly string[] _webPagePrefixArray;

        protected BaseHttpExtractor(IConfigurationManager manager, ILogger logger, IHttpClientFactory httpClientFactory, string webPagePrefix)
            : base(manager, httpClientFactory)
        {
            this._logger = logger;
            _webPagePrefixArray = webPagePrefix.Split(';').Where(e => !string.IsNullOrEmpty(e)).ToArray();
        }

        public abstract string Name { get; }
        public virtual int Order => (int)ProviderOrder.Default;
        public virtual async Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPageUrl, CancellationToken token = default)
        {
            var result = new List<BaseDownloadableItem>();
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Get, webPageUrl);
                var title = string.Empty;
                req.Version = HttpVersion.Version20;
                var resp = await GetHttpClient().SendAsync(req, token);
                resp.EnsureSuccessStatusCode();
                var html = await resp.Content.ReadAsStringAsync();
                var m3u8Sources = new List<string>();
                if (this is IRegexExtractor regex)
                {
                    title = regex.GetTitleFromHtml(html);
                    var sources = regex.GetM3U8Sources(html);
                    m3u8Sources.AddRange(sources);
                }
                else if (this is IDOMExtractor dOMExtractor)
                {
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(html);
                    title = dOMExtractor.GetTitle(htmlDoc.DocumentNode);
                    var sources = dOMExtractor.GetM3U8Sources(htmlDoc.DocumentNode);
                    m3u8Sources.AddRange(sources);
                }
                foreach (var source in m3u8Sources)
                {
                    var quality = MediaQuality.Low;
                    if (source.Contains("480"))
                    {
                        quality = MediaQuality.Medium;
                    }
                    else if (source.Contains("720"))
                    {
                        quality = MediaQuality.High;
                    }
                    else if (source.Contains("1080"))
                    {
                        quality = MediaQuality.VeryHigh;
                    }

                    result.Add(new M3U8Item(title, source, GetRequestHeader(html), quality, title) { OrignalLink = webPageUrl });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: "failed to fetach downloadable in webpage {webPageUrl}", webPageUrl);
            }

            return result;

        }

        protected virtual Dictionary<string, string> GetRequestHeader(string html) => null;

        public virtual bool Support(string webPage)
        {
            return !string.IsNullOrEmpty(webPage) && _webPagePrefixArray.Any(webPage.StartsWith);
        }
    }
}
