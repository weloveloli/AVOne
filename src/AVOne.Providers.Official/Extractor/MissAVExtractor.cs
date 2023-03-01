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
    using AVOne.Constants;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Providers.Extractor;
    using Jint;
    using Microsoft.Extensions.Logging;

    public partial class MissAVExtractor : IMediaExtractorProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MissAVExtractor> _logger;
        private readonly Regex _regex;
        private readonly Regex _titleRegex;

        public static Dictionary<string, string> HeaderForMissAV =
            new Dictionary<string, string> {
                { "referer", "https://missav.com" },
                { "origin", "https://missav.com" }
            };

        public string Name => "Official";

        public int Order => 1;
        public MissAVExtractor(IHttpClientFactory httpClientFactory, ILogger<MissAVExtractor> logger)
        {
            _httpClient = httpClientFactory?.CreateClient(AVOneConstants.Default) ?? new HttpClient();
            _logger = logger;
            // The regex pattern to match lines starting with eval
            var pattern = @"^eval\((.*)\)$";
            // The regex options to enable multiline mode
            var options = RegexOptions.Multiline;
            // Create a regex object with the pattern and options
            _regex = new Regex(pattern, options);
            _titleRegex = GetTitleRegex();
        }

        public async Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPageUrl, CancellationToken token = default)
        {
            var sources = Enumerable.Empty<string>();
            var result = new List<BaseDownloadableItem>();
            try
            {
                var resp = await _httpClient.GetAsync(webPageUrl, token);
                resp.EnsureSuccessStatusCode();
                var html = await resp.Content.ReadAsStringAsync();
                var title = GetTitleFromHtml(html, _titleRegex);
                var isChineseSub = html.Contains("<a href=\"https://missav.com/chinese-subtitle\" class=\"text-nord13 font-medium\">中文字幕</a>");
                var avId = title[..title.IndexOf(" ")] + (isChineseSub ? "-C" : string.Empty);
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

                    result.Add(new M3U8Item(avId, source, HeaderForMissAV, quality, title));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to fetach downloadable in webpage {0}", webPageUrl);
            }

            return result;
        }

        public static Regex GetTitleRegex()
        {
            return TitleRegex();
        }

        // A method to fetch the title from a HTML string
        public static string GetTitleFromHtml(string html, Regex regex)
        {
            // Check if the html string is null or empty
            if (string.IsNullOrEmpty(html))
            {
                // Return an empty string
                return "";
            }

            // Find the first match in the html string
            var match = regex.Match(html);

            // Check if the match is successful
            if (match.Success)
            {
                // Return the value of the first group (the title text)
                return match.Groups[1].Value;
            }
            else
            {
                // Return an empty string if no match is found
                return "";
            }
        }

        public IEnumerable<string> GetSources(string html)
        {
            var lines = html.Split('\n');
            var script = string.Empty;
            foreach (var item in lines)
            {
                if (item.Trim().StartsWith("eval"))
                {
                    var match = _regex.Match(item.Trim());
                    if (match.Success)
                    {
                        script = match.Groups[1].Value;
                    }
                }
            }
            if (string.IsNullOrEmpty(script))
            {
                throw new Exception("eval script can not be found in the webpage");
            }

            var engine = new Engine();
            engine.Execute("var value = " + script);
            var scourceStr = engine.GetValue("value").AsString();

            foreach (var line in scourceStr.Split(";"))
            {
                var start = line.IndexOf("'") + 1;
                var end = line.LastIndexOf("'") - 1;
                if (end <= start) continue;
                yield return line.Substring(start, end - start + 1);
            }
        }

        public bool Support(string webPage)
        {
            return webPage.StartsWith("https://missav.com");
        }

        [GeneratedRegex("<title>(.*?)</title>", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex TitleRegex();
    }
}
