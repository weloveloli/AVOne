// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractors.Base
{
    using AVOne.Enum;
    using AVOne.Extensions;
    using AVOne.Models.Download;
    using AVOne.Providers.Extractor;
    using AVOne.Providers.Official.Common;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;

    public abstract class BaseHttpExtractor : IMediaExtractorProvider
    {
        protected ILogger _logger;
        private readonly string[] _webPagePrefixArray;
        private readonly IHttpHelper _httpHelper;

        protected BaseHttpExtractor(IHttpHelper httpHelper, ILoggerFactory loggerFactory, string webPagePrefix)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _webPagePrefixArray = webPagePrefix.Split(';').Where(e => !string.IsNullOrEmpty(e)).ToArray();
            _httpHelper = httpHelper;
        }

        public abstract string Name { get; }
        public virtual int Order => (int)ProviderOrder.Default;
        public virtual async Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPageUrl, CancellationToken token = default)
        {
            webPageUrl = NormalizeUrl(webPageUrl);
            var result = new List<BaseDownloadableItem>();
            try
            {
                var html = await _httpHelper.GetHtmlAsync(webPageUrl, token);
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

        /// <summary>
        /// Normalize the url, some website may have different url for the same page.
        /// </summary>
        /// <param name="webPageUrl"></param>
        /// <returns></returns>
        protected virtual string NormalizeUrl(string webPageUrl)
        {
            return webPageUrl;
        }
    }
}
