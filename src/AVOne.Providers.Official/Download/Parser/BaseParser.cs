// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Parser
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    internal class BaseParser
    {
        protected Dictionary<string, string> ParseAttributes(string attributes)
        {
            var result = new Dictionary<string, string>();
            var matches = Regex.Matches(attributes,
                @"([^=]*)=((?:"".*?"",)|(?:.*?,)|(?:.*?$))");
            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value.Trim();
                var val = match.Groups[2].Value.Trim();
                val = Regex.Replace(val, @"^['""]?(.*?)['""]?[,]?$", "$1");
                result[key] = val;
            }
            return result;
        }
    }
}
