// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractors.Embed
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Enum;
    using AVOne.Extensions;
    using AVOne.Models.Download;
    using AVOne.Providers.Official.Common;
    using AVOne.Providers.Official.Extractors.Base;
    using Microsoft.Extensions.Logging;

    internal class EmbedTKtubeExtractor : BaseHttpExtractor, IRegexExtractor, IEmbedInnerExtractor
    {
        private const string WebPagePrefix = "https://tktube.com/embed/";

        public EmbedTKtubeExtractor(IHttpHelper httpHelper, ILoggerFactory loggerFactory)
        : base(httpHelper, loggerFactory, WebPagePrefix)
        {
        }

        public override string Name => "TKtubeEmbeded";

        public Task<IEnumerable<BaseDownloadableItem>> ExtractFromEmbedPageAsync(string parnentWebPageUrl, string parentHtmlContent, string embedWebPageUrl, string embedHtmlContent, CancellationToken token = default)
        {
            return Task.FromResult(GetItems(GetTitle(embedHtmlContent), embedHtmlContent, embedWebPageUrl));
        }

        public IEnumerable<BaseDownloadableItem> GetItems(string title, string html, string url)
        {
            // for each lines in html, find the line that contains "flashvars"
            // then get the the flashvars value

            var lines = html.Split('\n');
            var lineNo = -1;
            var i = -1;
            foreach (var line in lines)
            {
                i++;
                if (line.Contains("flashvars"))
                {
                    lineNo = i;
                    break;
                }
            }
            if (lineNo == -1)
            {
                yield break;
            }
            var flashvars = lines[lineNo + 1];
            var rndMatch = Regex.Match(flashvars, @"rnd: '(\d+)'");
            if (!rndMatch.Success)
            {
                yield break;
            }

            var rnd = rndMatch.Groups[1].Value;
            var lMatch = Regex.Match(flashvars, @"license_code: '(\S\d+)'");
            if (!lMatch.Success)
            {
                yield break;
            }
            var license = lMatch.Groups[1].Value;

            for (var index = 0; index <= 2; index++)
            {
                var prefix = index > 1 ? "" + index : "";
                var mid = index == 0 ? "_" : "_alt_";
                var videUrlPattern = $"video{mid}url{prefix}: '(.+?)'";
                var videoTextPattern = $"video{mid}url{prefix}_text: '(.+?)'";
                var videoMatch = Regex.Match(flashvars, videUrlPattern);
                var videoTextMatch = Regex.Match(flashvars, videoTextPattern);

                if (!videoMatch.Success)
                {
                    continue;
                }
                var mp4Url = videoMatch.Groups[1].Value;
                mp4Url = TKtubeEmbededExtractorUtils.GetRealUrl(mp4Url, license);
                var item = new HttpItem(
                    title.EscapeFileName(),
                    mp4Url,
                    null,
                    GetMediaQuality(videoTextMatch),
                    title);
                yield return item;
            }
        }

        public MediaQuality GetMediaQuality(Match textMatch)
        {
            if (!textMatch.Success)
            {
                return MediaQuality.Medium;
            }
            var text = textMatch.Groups[1].Value;
            return text switch
            {
                "360p" => MediaQuality.Low,
                "480p" => MediaQuality.Medium,
                "720p" => MediaQuality.High,
                _ => MediaQuality.Medium,
            };
        }

        public string GetTitle(string html)
        {
            var start = html.IndexOf("<title>");
            var end = html.IndexOf("</title>");
            return html.Substring(start + 7, end - start - 7);
        }

        public bool IsEmbedUrlSupported(string embedUrl)
        {
            return embedUrl.StartsWith(WebPagePrefix);
        }
    }
}
