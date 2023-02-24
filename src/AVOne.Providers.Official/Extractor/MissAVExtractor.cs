// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Models.Download;
    using AVOne.Providers.Extractor;
    using Jint;
    using Microsoft.Extensions.Logging;

    public class MissAVExtractor : IMediaExtractorProvider
    {
        private HttpClient _httpClient;
        private ILogger<MissAVExtractor> _logger;
        private readonly Regex _regex;

        public string Name => "Official";

        public int Order => 1;
        public MissAVExtractor(IHttpClientFactory httpClientFactory, ILogger<MissAVExtractor> logger)
        {
            //_httpClient = httpClientFactory.CreateClient(AVOneConstants.Default);
            //_logger = logger;
            // The regex pattern to match lines starting with eval
            string pattern = @"eval\((.*)\)$";
            // The regex options to enable multiline mode
            RegexOptions options = RegexOptions.Multiline;
            // Create a regex object with the pattern and options
            _regex = new Regex(pattern, options);
        }

        public async Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPage, CancellationToken token = default)
        {
            try
            {
                var resp = await _httpClient.GetAsync(webPage, token);
                resp.EnsureSuccessStatusCode();
                var html = await resp.Content.ReadAsStringAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to fetach downloadable in webpage {0}", webPage);
            }
            return Enumerable.Empty<BaseDownloadableItem>();
        }

        public string GetSources(string html)
        {
            // The regex pattern to match lines starting with eval
            var match = _regex.Match(html);
            // If a match is found, print it
            if (!match.Success)
            {
                throw new Exception("eval script can not be found in the webpage");
            }
            var script = match.Groups[1].Value;

            var engine = new Engine();
            engine.Execute("var value = " + script);
            return engine.GetValue("value").AsString();
        }

        public bool Support(string webPage)
        {
            return webPage.StartsWith("https://missav.com");
        }
    }
}
