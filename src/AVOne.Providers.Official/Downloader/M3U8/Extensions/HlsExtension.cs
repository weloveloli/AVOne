// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.M3U8.Extensions
{
    using System.Text.RegularExpressions;
    using AVOne.Providers.Official.Downloader.M3U8.Parser;

    public static class HlsExtension
    {
        public static bool IsM3U8(this string manifest)
        {
            return manifest.Contains("#EXTM3U");
        }

        public static bool IsMPD(this string manifest)
        {
            return manifest.Contains("<MPD ") || manifest.Contains("<MPD>");
        }

        public static bool IsLive(this MediaPlaylist mediaPlaylist)
        {
            return !mediaPlaylist.EndList;
        }

        public static bool IsMaster(this string manifest)
        {
            return manifest.Contains("#EXT-X-STREAM-INF");
        }

        public static StreamInfo? GetWithHighestQuality(
            this IEnumerable<StreamInfo> source, int? maxHeight = null)
        {
            var _maxHeight = maxHeight ?? int.MaxValue;
            return source
                .Select(it => new
                {
                    quality = it.Resolution?.Height ?? 0,
                    item = it
                })
                .Where(it => it.quality <= _maxHeight)
                .OrderByDescending(it => it.quality)
                .ThenByDescending(it => it.item.Bandwidth)
                .FirstOrDefault()?
                .item;
        }

        public static string CombineUri(this string m3u8Url, string uri)
        {
            if (uri.StartsWith("http"))
                return uri;
            m3u8Url = Regex.Match(m3u8Url, @"(.*?\/)+").Value;
            return
                new Uri(new Uri(m3u8Url), uri).ToString();
        }
    }
}
