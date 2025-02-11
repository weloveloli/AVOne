// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractors.Base
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Enum;
    using AVOne.Extensions;
    using AVOne.Models.Download;
    using AVOne.Providers.Extractor;
    using AVOne.Providers.Official.Common;
    using Microsoft.Extensions.Logging;

    public abstract class BaseEmbedHttpExtractor : IMediaExtractorProvider
    {
        private readonly string[] _webPagePrefixArray;
        private readonly List<IMediaExtractorProvider> embedExtractor;
        protected readonly ILogger logger;
        private readonly IHttpHelper _httpHelper;

        protected BaseEmbedHttpExtractor(IHttpHelper httpHelper, ILoggerFactory loggerFactory, string webPagePrefix, params IMediaExtractorProvider[] embededExtractorProviders)
        {
            _webPagePrefixArray = webPagePrefix.Split(';').Where(e => !string.IsNullOrEmpty(e)).ToArray();
            embedExtractor = embededExtractorProviders.ToList();
            logger = loggerFactory.CreateLogger(GetType());
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
                var html = await _httpHelper.GetHtmlAsync(webPageUrl, token);
                links = GetEmbedPages(webPageUrl, html);
                foreach (var link in links)
                {
                    var extractor = GetEmbededExtractor(link);
                    if (extractor != null)
                    {
                        var items = await extractor.ExtractAsync(link, token);
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                item.OrignalLink = webPageUrl;
                                item.HasMetaData = TryExtractMetaData(item, html, webPageUrl);
                                result.Add(item);
                            }
                        }
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

        public virtual bool TryExtractMetaData(BaseDownloadableItem item, string html, string url)
        {
            return false;
        }

        public virtual IMediaExtractorProvider? GetEmbededExtractor(string url)
        {
            return embedExtractor.FirstOrDefault(p => p.Support(url));
        }

        public virtual bool Support(string webPage)
        {
            return !string.IsNullOrEmpty(webPage) && _webPagePrefixArray.Any(webPage.StartsWith);
        }
    }
}
