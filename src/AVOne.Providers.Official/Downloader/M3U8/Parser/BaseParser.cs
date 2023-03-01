// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.M3U8.Parser
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    internal partial class BaseParser
    {
        protected Dictionary<string, string> ParseAttributes(string attributes)
        {
            var result = new Dictionary<string, string>();
            var matches = OverallRegex().Matches(attributes);

            foreach (var match in matches.Cast<Match>())
            {
                var key = match.Groups[1].Value.Trim();
                var val = match.Groups[2].Value.Trim();
                val = BaseContentRegex().Replace(val, "$1");
                result[key] = val;
            }
            return result;
        }

        [GeneratedRegex("^['\"]?(.*?)['\"]?[,]?$")]
        private static partial Regex BaseContentRegex();

        [GeneratedRegex("([^=]*)=((?:\".*?\",)|(?:.*?,)|(?:.*?$))")]
        private static partial Regex OverallRegex();
    }
}
