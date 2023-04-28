// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Providers.Official.Extractor.Base;
    using Jint;
    using Microsoft.Extensions.Logging;

    public partial class MissAVExtractor : BaseHttpExtractor, IRegexExtractor
    {
        private const string ExtraTitle = " - MissAV.com | 免費高清AV在線看";
        private readonly Regex _regex;
        private readonly Regex _titleRegex;

        public static Dictionary<string, string> HeaderForMissAV =
            new()
            {
                { "referer", "https://missav.com" },
                { "origin", "https://missav.com" }
            };

        public override string Name => "MissAV";

        public MissAVExtractor(IConfigurationManager manager, IHttpClientFactory httpClientFactory, ILogger<MissAVExtractor> logger)
        : base(manager, logger, httpClientFactory, "https://missav.com")
        {
            // The regex pattern to match lines starting with eval
            var pattern = @"^eval\((.*)\)$";
            // The regex options to enable multiline mode
            var options = RegexOptions.Multiline;
            // Create a regex object with the pattern and options
            _regex = new Regex(pattern, options);
            _titleRegex = TitleRegex();
        }

        public IEnumerable<string> GetM3U8Sources(string html)
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
        [GeneratedRegex("<meta property=\"og:title\" content=\"(.*?)\" />", RegexOptions.IgnoreCase, "en-US")]
        public static partial Regex TitleRegex();
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
                var title = match.Groups[1].Value;

                if (title.Contains(ExtraTitle))
                {
                    title = title.Replace(ExtraTitle, "");
                }
                return title;
            }
            else
            {
                // Return an empty string if no match is found
                return "";
            }
        }

        public string GetTitle(string html)
        {
            return GetTitleFromHtml(html, _titleRegex);
        }

        protected override Dictionary<string, string> GetRequestHeader(string html)
        {
            return HeaderForMissAV;
        }

        public IEnumerable<BaseDownloadableItem> GetItems(string title, string html, string url)
        {
            var m3u8Sources = GetM3U8Sources(html);

            foreach (var source in m3u8Sources)
            {
                if (string.IsNullOrEmpty(source))
                {
                    continue;
                }

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

                yield return new M3U8Item(title, source, GetRequestHeader(html), quality, title) { OrignalLink = url };
            }
        }
    }
}
