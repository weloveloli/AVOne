// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.M3U8.Extensions
{
    internal static class PipeExtension
    {
        public static TResult Pipe<TSource, TResult>(this TSource source, Func<TSource, TResult> selector)
        {
            return selector(source);
        }
    }
}
