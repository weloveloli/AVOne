// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractors
{
    using System.Collections.Generic;
    using AVOne.Enum;
    using AVOne.Extensions;
    using AVOne.Models.Download;
    using AVOne.Providers.Official.Common;
    using AVOne.Providers.Official.Extractors.Base;
    using Fizzler.Systems.HtmlAgilityPack;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class TaiavExtrator : BaseHttpExtractor, IDOMExtractor
    {
        private const string TaiavWebSitePrefix = "https://taimadou.com";
        public TaiavExtrator(IHttpHelper httpHelper, ILoggerFactory loggerFactory) : base(httpHelper, loggerFactory, TaiavWebSitePrefix)
        {
        }

        public override int Order => 1;

        public override string Name => "Taiav";

        public IEnumerable<BaseDownloadableItem> GetItems(string title, HtmlNode node, string url)
        {
            // get the movie id from the url https://taimadou.com/cn/movie/653d8ebfdb328a26584f66a5
            var movieId = url.Split('/')[5];
            // call the api to get the video url
            var apiUrl = $"https://taimadou.com/api/getmovie?type=1280&id={movieId}";
            var jsonString = this._httpHelper.GetHtmlAsync(apiUrl, s_HeaderForTaiav).Result;
            // parse the json to get the video url
            var videoObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var videoUrl = videoObj?["m3u8"].ToString() ?? string.Empty;
            videoUrl = TaiavWebSitePrefix + videoUrl;
            var metadataObj = GetMetaDataObject(node);
            var thumbnailUrl = metadataObj?["thumbnailUrl"].ToString() ?? string.Empty;
            var item = new M3U8Item
            {
                Name = title,
                Url = videoUrl,
                Title = title,
                Header = s_HeaderForTaiav,
                Quality = MediaQuality.High,
                OriginalTitle = title,
                HomePageUrl = url,
                SaveName = title,
                OrignalLink = url
            };
            if (!string.IsNullOrEmpty(thumbnailUrl))
            {
                item.HasMetaData = true;
                item.AddImage(new Models.Info.ItemImageInfo
                {
                    Path = thumbnailUrl,
                    Type = ImageType.Primary
                });

                var tagNodes = node.QuerySelectorAll("div.uk-first-column a.rounded");

                if (!tagNodes.IsEmpty())
                {
                    item.Tags = Array.Empty<string>();
                }
                else
                {
                    item.Tags = tagNodes.Select(e => e.InnerText).ToArray();
                }
            }
            yield return item;
        }

        private static readonly Dictionary<string, string> s_HeaderForTaiav = new()
        {
            { "referer", TaiavWebSitePrefix },
            { "origin", TaiavWebSitePrefix }
        };

        private IDictionary<string, object> GetMetaDataObject(HtmlNode dom)
        {
            var content = dom.QuerySelector("script[type='application/ld+json']").InnerHtml;
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(content) ?? new Dictionary<string, object>();
        }

        public string GetTitle(HtmlNode dom)
        {
            var VideoObject = GetMetaDataObject(dom);
            var title = VideoObject?["name"].ToString() ?? string.Empty;
            if (title.Length > 30)
            {
                title = title.Substring(0, 30);
            }
            return title.Replace("/", " ").Replace("\\", " ");
        }
    }
}
