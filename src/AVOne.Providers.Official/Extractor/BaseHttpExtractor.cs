// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Helper;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Providers.Extractor;
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
        public abstract Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPageUrl, CancellationToken token = default);
        public virtual bool Support(string webPage)
        {
            return !string.IsNullOrEmpty(webPage) && _webPagePrefixArray.Any(webPage.StartsWith);
        }
    }
}
