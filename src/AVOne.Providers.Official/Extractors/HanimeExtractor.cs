﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractors
{
    using System.Collections.Generic;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Providers.Official.Common;
    using AVOne.Providers.Official.Extractors.Base;
    using Fizzler.Systems.HtmlAgilityPack;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class HanimeExtractor : BaseHttpExtractor, IDOMExtractor
    {
        private const string WebPagePrefix = "https://hanime1.me";

        public HanimeExtractor(IHttpHelper httpHelper, ILoggerFactory loggerFactory)
           : base(httpHelper, loggerFactory, WebPagePrefix)
        {
        }

        public override string Name => "hanime";

        public IEnumerable<BaseDownloadableItem> GetItems(string title, HtmlNode node, string url)
        {
            var sources = GetSources(node);

            foreach (var source in sources)
            {
                /// check if source is a m3u8 link
                if (source.Contains(".m3u8"))
                {
                    yield return new M3U8Item(title, source, null, MediaQuality.None, title) { OrignalLink = url };
                }
                else
                {
                    yield return new HttpItem(title, source, null, MediaQuality.None, title) { OrignalLink = url };
                }
            }

        }

        public IEnumerable<string> GetSources(HtmlNode dom)
        {
            var content = dom.QuerySelector("script[type='application/ld+json']").InnerHtml;
            var videoObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            if (videoObject != null)
            {
                yield return videoObject!["contentUrl"].ToString()!;
            }
        }

        public string GetTitle(HtmlNode dom)
        {
            var content = dom.QuerySelector("script[type='application/ld+json']").InnerHtml;
            var VideoObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            var title = VideoObject?["name"].ToString() ?? string.Empty;
            if (title.Length > 30)
            {
                title = title.Substring(0, 30);
            }
            return title.Replace("/", " ").Replace("\\", " ");
        }
    }
}
