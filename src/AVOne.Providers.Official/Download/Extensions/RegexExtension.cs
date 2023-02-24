// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    internal static class RegexExtension
    {
        public static IEnumerable<TResult> Select<TResult>(this MatchCollection matches, Func<Match, TResult> selector)
        {
            foreach (Match match in matches)
            {
                yield return selector(match);
            }
        }
    }
}
