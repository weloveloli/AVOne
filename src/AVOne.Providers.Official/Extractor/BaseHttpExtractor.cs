﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Models.Download;
    using AVOne.Providers.Extractor;
    using Microsoft.Extensions.Logging;

    public abstract partial class BaseHttpExtractor : IMediaExtractorProvider
    {
        protected HttpClient _httpClient;
        protected ILogger _logger;
        private readonly string _webPageStart;

        protected BaseHttpExtractor(ILogger logger, IHttpClientFactory httpClientFactory, string webPageStart)
        {

            this._httpClient = httpClientFactory.CreateClient(Constants.Official);
            this._logger = logger;
            _webPageStart = webPageStart;
        }

        public abstract string Name { get; }
        public abstract int Order { get; }

        [GeneratedRegex("<title>(.*?)</title>", RegexOptions.IgnoreCase, "en-US")]
        public static partial Regex TitleRegex();

        public abstract Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPage, CancellationToken token = default);
        public virtual bool Support(string webPage)
        {
            return !string.IsNullOrEmpty(webPage) && webPage.StartsWith(_webPageStart);
        }
    }
}
