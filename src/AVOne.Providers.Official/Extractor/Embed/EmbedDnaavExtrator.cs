// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Embeded
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Providers.Official.Extractor.Base;
    using HtmlAgilityPack;

    public class EmbedDnaavExtrator : IEmbedInnerExtractor
    {
        private const string WebPagePrefix = "https://www.dnaav.com/embed/";

        public Task<IEnumerable<BaseDownloadableItem>> ExtractFromEmbedPageAsync(
            string parnentWebPageUrl,
            string parentHtmlContent,
            string embedWebPageUrl,
            string embedHtmlContent,
            CancellationToken token = default)
        {
            // extract the title from the parent page
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(parentHtmlContent);
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
            var title = titleNode?.InnerText;
            // find the video url
            var scriptNode = htmlDoc.DocumentNode.SelectSingleNode("//script[contains(text(), 'new DPlayer')]");
            var scriptContent = scriptNode?.InnerText;
            // extract the video url from the script content
            var videoUrl = scriptContent?.Split("url:")[1].Split(",")[0].Trim().Trim('\'');
            var item = new M3U8Item
            {
                Title = title,
                Url = videoUrl,
                Quality = MediaQuality.None,
                SaveName = title,
            };
            // Get the video quality from the meta tag
            // meta property og:video:height
            var metaNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:video:height']");
            if (metaNode != null)
            {
                var quality = metaNode.GetAttributeValue("content", "0");
                if (int.TryParse(quality, out var qualityInt))
                {
                    item.Quality = qualityInt switch
                    {
                        720 => MediaQuality.High,
                        480 => MediaQuality.Medium,
                        360 => MediaQuality.Low,
                        _ => MediaQuality.None,
                    };
                }
            }
            // Get the video meta data from the meta tag
            // meta name keywords
            var keywordsNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='keywords']");
            // video image from the og:image meta tag
            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
            if (keywordsNode != null && imageNode != null)
            {
                item.HasMetaData = true;
                item.Genres = keywordsNode.GetAttributeValue("content", string.Empty).Split(",");
                item.AddImage(new Models.Info.ItemImageInfo
                {
                    Path = imageNode.GetAttributeValue("content", string.Empty),
                    Type = ImageType.Primary
                });
            }

            return Task.FromResult<IEnumerable<BaseDownloadableItem>>(new[] { item });
        }

        /// <summary>
        /// Whether the embed url is supported, used to judge if the emebed provider should be choosen or not
        /// </summary>
        /// <param name="embedUrl"></param>
        /// <returns></returns>
        public bool IsEmbedUrlSupported(string embedUrl)
        {
            return embedUrl.StartsWith(WebPagePrefix);
        }
    }
}
