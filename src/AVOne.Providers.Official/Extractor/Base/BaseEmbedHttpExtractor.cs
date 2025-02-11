// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Base
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Enum;
    using AVOne.Extensions;
    using AVOne.Models.Download;
    using AVOne.Providers.Extractor;
    using Microsoft.Extensions.Logging;

    public abstract class BaseEmbedHttpExtractor : IMediaExtractorProvider
    {
        private readonly string[] _webPagePrefixArray;
        private readonly List<IEmbedInnerExtractor> embedExtractor;
        protected readonly ILogger logger;
        private readonly IHttpHelper _httpHelper;

        protected BaseEmbedHttpExtractor(IHttpHelper httpHelper, ILoggerFactory loggerFactory, string webPagePrefix, params IEmbedInnerExtractor[] embededExtractorProviders)
        {
            _webPagePrefixArray = webPagePrefix.Split(';').Where(e => !string.IsNullOrEmpty(e)).ToArray();
            this.embedExtractor = embededExtractorProviders.ToList();
            this.logger = loggerFactory.CreateLogger(this.GetType());
            _httpHelper = httpHelper;
        }

        public abstract string Name { get; }

        public int Order => (int)ProviderOrder.Default;

        public async Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPageUrl, CancellationToken token = default)
        {
            var links = Enumerable.Empty<string>();
            var result = new List<BaseDownloadableItem>();
            try
            {
                var html = await this._httpHelper.GetHtmlAsync(webPageUrl, token);
                links = this.GetEmbedPages(webPageUrl, html);
                foreach (var link in links)
                {
                    var extractor = GetEmbededExtractor(link);
                    if (extractor != null)
                    {
                        var embedHtml = await this._httpHelper.GetHtmlAsync(link, token);
                        var items = await extractor.ExtractFromEmbedPageAsync(webPageUrl, html, link, embedHtml, token);
                        result.AddRange(items);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, message: "failed to fetach downloadable in webpage {webPageUrl}", webPageUrl);
            }
            return result;
        }

        public abstract IEnumerable<string> GetEmbedPages(string url, string html);
        public virtual IEmbedInnerExtractor? GetEmbededExtractor(string url)
        {
            return embedExtractor.FirstOrDefault(p => p.IsEmbedUrlSupported(url));
        }

        public virtual bool Support(string webPage)
        {
            return !string.IsNullOrEmpty(webPage) && _webPagePrefixArray.Any(webPage.StartsWith);
        }
    }
}
