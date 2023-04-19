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
    using AVOne.Models.Download;
    using AVOne.Providers.Extractor;
    using Microsoft.Extensions.Logging;

    public abstract class BaseHttpExtractor : HttpClientHelper, IMediaExtractorProvider
    {
        protected ILogger _logger;
        private readonly string _webPageStart;

        protected BaseHttpExtractor(IConfigurationManager manager, ILogger logger, IHttpClientFactory httpClientFactory, string webPageStart)
            : base(manager, httpClientFactory)
        {
            this._logger = logger;
            _webPageStart = webPageStart;
        }

        public abstract string Name { get; }
        public abstract int Order { get; }

        public abstract Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPageUrl, CancellationToken token = default);
        public virtual bool Support(string webPage)
        {
            return !string.IsNullOrEmpty(webPage) && webPage.StartsWith(_webPageStart);
        }
    }
}
