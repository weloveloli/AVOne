// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractors
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Constants;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Providers.Extractor;
    using AVOne.Providers.Official.Common;
    using Microsoft.Extensions.Logging;

    public class XChinaExtractor : IMediaExtractorProvider
    {
        private readonly IHttpHelper _httpHelper;
        private readonly ILogger<XChinaExtractor> _logger;

        public string Name => "xchina";

        public int Order => 1;

        public XChinaExtractor(IHttpHelper httpHelper, ILogger<XChinaExtractor> logger)
        {
            _httpHelper = httpHelper;
            _logger = logger;
        }

        private record VideoObj(string name, List<string> thumbnailUrl);

        public async Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPageUrl, CancellationToken token = default)
        {
            var htmlContent = await _httpHelper.GetHtmlAsync(webPageUrl, token);
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            var title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText;
            // get the last script node with type=application/ld+json in the head tag
            var scriptNode = htmlDoc.DocumentNode.SelectSingleNode("//head/script[@type='application/ld+json'][last()]");
            var content = scriptNode?.InnerText;
            if (string.IsNullOrEmpty(content))
            {
                _logger.LogError("Failed to find the script node with type=application/ld+json in the head tag");
                return new List<BaseDownloadableItem>();
            }

            //parse the json content
            var videoObject = Newtonsoft.Json.JsonConvert.DeserializeObject<VideoObj>(content);
            var thumnailUrl = "";
            if (videoObject != null)
            {
                title = videoObject.name;
                thumnailUrl = videoObject.thumbnailUrl?.FirstOrDefault();
            }
            // find the script node which comes after the div tag with id=video_player
            scriptNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='video_player']/following-sibling::script[1]");
            // use regex to extract the video url
            var videoUrl = System.Text.RegularExpressions.Regex.Match(scriptNode.InnerText, @"src:\s*'(.*?)'").Groups[1].Value;

            // find the actor name
            var actorNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='actorsOrModels']");
            // the actor name is in the a tag or wrapped by a a tag
            var actorName = actorNode?.SelectSingleNode("a")?.InnerText ?? actorNode?.InnerText;

            // get the file name, the filename is the inner text of a div which also contains i with class=fa-file-o 
            var fileNameNode = htmlDoc.DocumentNode.SelectSingleNode("//i[@class='fa fa-file-o']/parent::div");
            var fileName = fileNameNode?.InnerText;
            var name = $"{fileName} {title}";
            var item = new M3U8Item
            {
                Title = title,
                SaveName = fileName,
                OrignalLink = webPageUrl,
                Name = name,
                OriginalTitle = name,
                Quality = MediaQuality.High,
                Url = videoUrl,
                Header = new Dictionary<string, string>
                {
                    { "Referer", webPageUrl }
                }
            };
            if (thumnailUrl != null && !string.IsNullOrEmpty(actorName))
            {
                var imageFileName = await _httpHelper.DownloadThumbAsync(thumnailUrl, new Dictionary<string, string> { { "Referer", webPageUrl } }, token);

                item.AddPerson(new Models.Info.PersonInfo
                {
                    Name = actorName,
                    Type = PersonType.Actor
                });

                item.AddImage(new Models.Info.ItemImageInfo
                {
                    Path = imageFileName,
                    Type = ImageType.Primary
                });

                item.HasMetaData = true;
            }

            return new List<BaseDownloadableItem> { item };
        }

        public bool Support(string webPageUrl)
        {
            return webPageUrl.StartsWith("https://xchina.co/");
        }
    }
}
