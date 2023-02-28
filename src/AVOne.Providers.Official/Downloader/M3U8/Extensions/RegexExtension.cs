// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.M3U8.Extensions
{
    using System.Text.RegularExpressions;

    internal static class RegexExtension
    {
        public static IEnumerable<TResult> Select<TResult>(this MatchCollection matches, Func<Match, TResult> selector)
        {
            return from Match match in matches
                   select selector(match);
        }
    }
}
