// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using AVOne.Configuration;
    using Furion.JsonSerialization;
    using Microsoft.Extensions.Logging;

    public partial class AV51ClubExtrator : BaseHttpExtractor, IRegexExtractor
    {
        public AV51ClubExtrator(IConfigurationManager manager, ILogger<AV51ClubExtrator> logger, IHttpClientFactory httpClientFactory) : base(manager, logger, httpClientFactory, "https://51av.club")
        {
            _titleRegex = TitleRegex();
            _scriptRegex = ScriptRegex();
        }
        public override string Name => "51AV";
        private readonly Regex _titleRegex;
        private readonly Regex _scriptRegex;
        [GeneratedRegex("<p class=\"name\">(.*?)<i class=.*", RegexOptions.IgnoreCase, "en-US")]
        public static partial Regex TitleRegex();

        [GeneratedRegex("<script type=\"text\\/javascript\">var player_aaaa=(\\{.+\\})<\\/script>", RegexOptions.Multiline | RegexOptions.IgnoreCase, "en-US")]
        public static partial Regex ScriptRegex();

        // A method to fetch the title from a HTML string
        public static string GetStringFromHtml(string html, Regex regex)
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
                return title;
            }
            else
            {
                // Return an empty string if no match is found
                return "";
            }
        }

        public IEnumerable<string> GetSources(string html)
        {
            var data = GetStringFromHtml(html, _scriptRegex);
            var dataJson = JSON.Deserialize<Dictionary<string, object>>(data);
            var m3u8Link = dataJson["url"];
            if (m3u8Link != null)
            {
                return new List<string> { m3u8Link!.ToString()! };
            }
            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetM3U8Sources(string html)
        {
            var data = GetStringFromHtml(html, _scriptRegex);
            var dataJson = JSON.Deserialize<Dictionary<string, object>>(data);
            var m3u8Link = dataJson["url"];
            if (m3u8Link != null)
            {
                return new List<string> { m3u8Link!.ToString()! };
            }
            return Enumerable.Empty<string>();
        }

        public string GetTitleFromHtml(string html)
        {
            return GetStringFromHtml(html, _titleRegex);
        }
    }
}
