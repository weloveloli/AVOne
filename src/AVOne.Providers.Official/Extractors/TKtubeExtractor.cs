// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractors
{
    using System;
    using System.Collections.Generic;
    using AVOne.Constants;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Providers.Official.Common;
    using AVOne.Providers.Official.Extractors.Base;
    using AVOne.Providers.Official.Extractors.Embeded;
    using Fizzler.Systems.HtmlAgilityPack;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;

    public class TKtubeExtractor : BaseEmbedHttpExtractor
    {
        public TKtubeExtractor(IHttpHelper httphelper, ILoggerFactory loggerFactory)
            : base(httphelper, loggerFactory, "https://tktube.com", new TKtubeEmbededExtractor(httphelper, loggerFactory))
        {
        }

        public override string Name => "TKtube";

        public override IEnumerable<string> GetEmbedPages(string url, string html)
        {
            var videoId = GetVideoId(url);
            var embedUrl = $"https://tktube.com/embed/{videoId}";
            return new List<string> { embedUrl };
        }

        // add a function that extract 121939 from https://tktube.com/videos/121939/1854/
        public string GetVideoId(string url)
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var parts = path.Split('/');
            return parts[2];
        }

        public override bool TryExtractMetaData(BaseDownloadableItem item, string html, string url)
        {
            try
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                item.HomePageUrl = url;

                var title = htmlDoc.DocumentNode.QuerySelector("title").InnerHtml;
                item.Name = title;
                item.OriginalTitle = title;

                var imageUrl = htmlDoc.DocumentNode.QuerySelector("meta[property='og:image']").Attributes["content"].Value;
                item.AddImage(new Models.Info.ItemImageInfo
                {
                    Type = ImageType.Primary,
                    Path = imageUrl,
                });
                item.AddImage(new Models.Info.ItemImageInfo
                {
                    Type = ImageType.Thumb,
                    Path = imageUrl.Replace("720p", "360p"),
                });

                var detailItems = htmlDoc.DocumentNode.QuerySelectorAll("div.block-details .item");

                foreach (var detailItem in detailItems)
                {
                    var text = detailItem.InnerText.Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "");
                    if (text.StartsWith("類別"))
                    {
                        item.Tags = text.Split("\t").Where(e => e.Trim() != string.Empty).Skip(1).ToArray();
                    }

                    if (text.StartsWith("標籤"))
                    {
                        item.Genres = text.Split("\t").Where(e => e.Trim() != string.Empty).Skip(1).ToArray();
                    }

                    if (text.StartsWith("女優"))
                    {
                        var actors = text.Split("\t").Where(e => e.Trim() != string.Empty).Skip(1).ToArray();
                        foreach (var actor in actors)
                        {
                            item.AddPerson(new Models.Info.PersonInfo
                            {
                                Name = actor,
                                Type = PersonType.Actor
                            });
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to fill metadata for {0}", url);
                return false;
            }
        }
    }
}
