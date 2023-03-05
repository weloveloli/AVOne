// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using Microsoft.Extensions.Logging;

    public class AvpleTvExtractor : BaseHttpExtractor
    {
        public override string Name => "AVPLE";

        public override int Order => 1;

        public AvpleTvExtractor(IHttpClientFactory httpClientFactory,
            ILogger<AvpleTvExtractor> logger)
            : base(logger, httpClientFactory, "https://avple.tv") { }

        public override async Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPageUrl, CancellationToken token = default)
        {
            var sources = Enumerable.Empty<string>();
            var result = new List<BaseDownloadableItem>();
            try
            {
                var resp = await _httpClient.GetAsync(webPageUrl, token);
                resp.EnsureSuccessStatusCode();
                var html = await resp.Content.ReadAsStringAsync();
                var title = GetTitleFromHtml(html, TitleRegex());
                var info = PornMovieInfo.GetId(null, title, out var _, out var _);
                sources = GetSources(html);
                foreach (var source in sources)
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

                    result.Add(new M3U8Item(string.IsNullOrEmpty(info) ? title : info, source, new Dictionary<string, string> { }, quality, title));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to fetach downloadable in webpage {0}", webPageUrl);
            }

            return result;
        }

        public IEnumerable<string> GetSources(string html)
        {
            foreach (var line in html.Split(new[] { '\r', '\n' }))
            {
                if (line.Trim().StartsWith("const source ="))
                {
                    var s = line.Trim();
                    var start = s.IndexOf("'") + 1;
                    var end = s.LastIndexOf("'") - 1;
                    yield return s[start..end];
                }
            }
        }

        // A method to fetch the title from a HTML string
        public static string GetTitleFromHtml(string html, Regex regex)
        {
            // Check if the html string is null or empty
            if (string.IsNullOrEmpty(html))
            {
                // Return an empty string
                return string.Empty;
            }

            // Find the first match in the html string
            var match = regex.Match(html);

            // Check if the match is successful
            if (match.Success)
            {
                // Return the value of the first group (the title text)
                var title = match.Groups[1].Value;

                if (title.Contains('|'))
                {
                    title = title[..title.IndexOf("|")].Trim();
                }
                return title;
            }
            else
            {
                // Return an empty string if no match is found
                return string.Empty;
            }
        }
    }
}
