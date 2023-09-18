// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Base
{
    using System.Net;
    using AVOne.Common.Helper;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Extensions;
    using AVOne.Models.Download;
    using AVOne.Providers.Extractor;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;

    public abstract class BaseHttpExtractor : HttpClientHelper, IMediaExtractorProvider
    {
        protected ILogger _logger;
        private readonly string[] _webPagePrefixArray;

        protected BaseHttpExtractor(IConfigurationManager manager, ILogger logger, IHttpClientFactory httpClientFactory, string webPagePrefix)
            : base(manager, httpClientFactory, logger)
        {
            _logger = logger;
            _webPagePrefixArray = webPagePrefix.Split(';').Where(e => !string.IsNullOrEmpty(e)).ToArray();
        }

        public abstract string Name { get; }
        public virtual int Order => (int)ProviderOrder.Default;
        public virtual async Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPageUrl, CancellationToken token = default)
        {
            webPageUrl = NormalizeUrl(webPageUrl);
            var result = new List<BaseDownloadableItem>();
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Get, webPageUrl);

                req.Version = HttpVersion.Version20;
                var resp = await GetHttpClient().SendAsync(req, token);
                resp.EnsureSuccessStatusCode();
                var html = await resp.Content.ReadAsStringAsync(token);
                if (this is IRegexExtractor regex)
                {
                    var title = regex.GetTitle(html).EscapeFileName();
                    return regex.GetItems(title, html, webPageUrl);
                }
                else if (this is IDOMExtractor dOMExtractor)
                {
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(html);
                    var title = dOMExtractor.GetTitle(htmlDoc.DocumentNode).EscapeFileName();
                    return dOMExtractor.GetItems(title, htmlDoc.DocumentNode, webPageUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, message: "failed to fetach downloadable in webpage {webPageUrl}", webPageUrl);
            }

            return result;

        }

        protected virtual Dictionary<string, string>? GetRequestHeader(string html) => null;

        public virtual bool Support(string webPage)
        {
            return !string.IsNullOrEmpty(webPage) && _webPagePrefixArray.Any(webPage.StartsWith);
        }

        protected virtual string NormalizeUrl(string webPageUrl)
        {
            return webPageUrl;
        }
    }
}
