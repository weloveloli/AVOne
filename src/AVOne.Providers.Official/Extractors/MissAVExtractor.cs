// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractors
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using AVOne.Common.Extensions;
    using AVOne.Constants;
    using AVOne.Enum;
    using AVOne.Extensions;
    using AVOne.Models.Download;
    using AVOne.Providers.Official.Common;
    using AVOne.Providers.Official.Extractors.Base;
    using Fizzler.Systems.HtmlAgilityPack;
    using HtmlAgilityPack;
    using Jint;
    using Microsoft.Extensions.Logging;

    public partial class MissAVExtractor : BaseHttpExtractor, IRegexExtractor
    {
        private const string ExtraTitle = " - MissAV.com | 免費高清AV在線看";
        private const string WebPagePrefix = "https://missav.ws";
        private readonly Regex _regex;
        private readonly Regex _titleRegex;

        public static Dictionary<string, string> HeaderForMissAV =
            new()
            {
                { "referer", "https://missav.ws" },
                { "origin", "https://missav.ws" }
            };

        public override string Name => "MissAV";

        public MissAVExtractor(IHttpHelper httpHelper, ILoggerFactory loggerFactory)
           : base(httpHelper, loggerFactory, WebPagePrefix)
        {
            // The regex pattern to match lines starting with eval
            var pattern = @"^eval\((.*)\)$";
            // The regex options to enable multiline mode
            var options = RegexOptions.Multiline;
            // Create a regex object with the pattern and options
            _regex = new Regex(pattern, options);
            _titleRegex = TitleRegex();
        }

        public IEnumerable<string> GetM3U8Sources(string html)
        {
            var lines = html.Split('\n');
            var script = string.Empty;
            foreach (var item in lines)
            {
                if (item.Trim().StartsWith("eval"))
                {
                    var match = _regex.Match(item.Trim());
                    if (match.Success)
                    {
                        script = match.Groups[1].Value;
                    }
                }
            }
            if (string.IsNullOrEmpty(script))
            {
                throw new Exception("eval script can not be found in the webpage");
            }

            var engine = new Engine();
            engine.Execute("var value = " + script);
            var scourceStr = engine.GetValue("value").AsString();

            var set = new HashSet<string>();
            foreach (var line in scourceStr.Split(";"))
            {
                var start = line.IndexOf("'") + 1;
                var end = line.LastIndexOf("'") - 1;
                if (end <= start) continue;
                set.Add(line.Substring(start, end - start + 1));
            }

            return set;
        }

        [GeneratedRegex("<meta property=\"og:title\" content=\"(.*?)\" />", RegexOptions.IgnoreCase, "en-US")]
        public static partial Regex TitleRegex();
        // A method to fetch the title from a HTML string
        public static string GetTitleFromHtml(string html, Regex regex)
        {
            // Check if the html string is null or empty
            if (string.IsNullOrEmpty(html))
            {
                // Return an empty string
                return "";
            }

            // Find the first match in the html string
            var match = regex.Match(html);

            // Check if the match is successful
            if (match.Success)
            {
                // Return the value of the first group (the title text)
                var title = match.Groups[1].Value;

                if (title.Contains(ExtraTitle))
                {
                    title = title.Replace(ExtraTitle, "");
                }
                return title;
            }
            else
            {
                // Return an empty string if no match is found
                return "";
            }
        }

        public string GetTitle(string html)
        {
            return GetTitleFromHtml(html, _titleRegex);
        }

        protected override Dictionary<string, string> GetRequestHeader(string html)
        {
            return HeaderForMissAV;
        }

        public IEnumerable<BaseDownloadableItem> GetItems(string title, string html, string url)
        {
            var m3u8Sources = GetM3U8Sources(html);
            foreach (var source in m3u8Sources)
            {
                if (string.IsNullOrEmpty(source))
                {
                    continue;
                }

                var quality = MediaQuality.Low;
                if (source.Contains("480"))
                {
                    quality = MediaQuality.Medium;
                }
                else if (source.Contains("720"))
                {
                    quality = MediaQuality.High;
                }
                else if (source.Contains("1080"))
                {
                    quality = MediaQuality.VeryHigh;
                }
                var item = new M3U8Item(title, source, GetRequestHeader(html), quality, title) { OrignalLink = url, HasMetaData = false };

                var hasMetaData = TryExtractMetaData(url, html, item);
                item.HasMetaData = hasMetaData;
                yield return item;
            }
        }

        protected override string NormalizeUrl(string url)
        {
            //return string.Concat("https://missav.com", url.AsSpan(url.LastIndexOf('/')));
            return url;
        }

        private static bool TryExtractMetaData(string url, string html, M3U8Item meta)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            meta.HomePageUrl = url;

            var texts = htmlDoc.DocumentNode.QuerySelectorAll("div[x-show*='video_details']  div.text-secondary");

            if (texts.IsEmpty())
            {
                return false;
            }
            if (!GetImage(htmlDoc, meta))
            {
                return false;
            }

            var name = string.Empty;
            var code = string.Empty;
            foreach (var textNode in texts)
            {
                var text = textNode.InnerText.Trim();
                if (string.IsNullOrEmpty(text)) continue;
                else if (text.StartsWith("發行日期") || text.StartsWith("发行日期") || text.StartsWith("配信開始日"))
                {
                    if (DateTime.TryParse(GetContent(text), default, out var date))
                    {
                        meta.ProductionYear = date.GetValidYear();
                    }
                }
                else if (text.StartsWith("番號") || text.StartsWith("品番") || text.StartsWith("番号"))
                {
                    code = GetContent(text);
                }

                else if (text.StartsWith("標題") || text.StartsWith("标题"))
                {
                    name = GetContent(text);
                }

                else if (text.StartsWith("女優") || text.StartsWith("女优"))
                {
                    var actors = GetContent(text).Split(",").Select(GetRealActor);
                    foreach (var actorName in actors)
                    {
                        meta.AddPerson(new Models.Info.PersonInfo
                        {
                            Name = actorName,
                            Type = PersonType.Actor
                        });
                    }
                }

                else if (text.StartsWith("導演") || text.StartsWith("导演") || text.StartsWith("監督"))
                {
                    var directors = GetContent(text).Split(",").Select(GetRealActor);
                    foreach (var directorName in directors)
                    {
                        meta.AddPerson(new Models.Info.PersonInfo
                        {
                            Name = directorName,
                            Type = PersonType.Director
                        });
                    }
                }

                else if (text.StartsWith("類型") || text.StartsWith("ジャンル") || text.StartsWith("类型"))
                {
                    meta.Genres = GetContent(text).Split(",").Select(e => e.Trim('\0', ' ', '\n', '\t')).ToArray();
                }

                else if (text.StartsWith("標籤") || text.StartsWith("レーベル") || text.StartsWith("タグ"))
                {
                    meta.Tags = GetContent(text).Split(",").Select(e => e.Trim('\0', ' ', '\n', '\t')).ToArray();
                }

                else if (text.StartsWith("發行商") || text.StartsWith("メーカー") || text.StartsWith("发行商"))
                {
                    meta.Studios = GetContent(text).Split(",").Select(e => e.Trim('\0', ' ', '\n', '\t')).ToArray();
                }
            }
            meta.Name = $"{code} {name}";
            meta.OriginalTitle = name;
            var overview = htmlDoc.DocumentNode.QuerySelector("div[x-show*='video_details']  div.text-secondary.break-all");

            if (overview != null)
            {
                meta.Overview = overview.InnerText.Trim('\0', ' ', '\n', '\t');
            }
            if (string.IsNullOrEmpty(meta.Name))
            {
                return false;
            }
            return true;
        }

        private static bool GetImage(HtmlDocument htmlDoc, M3U8Item meta)
        {
            var imageUrl = htmlDoc.DocumentNode.QuerySelector("link[as='image']")?.Attributes["href"]?.Value;
            if (string.IsNullOrEmpty(imageUrl)) return false;
            var primaryImage = string.Empty;
            var thumbnailImage = string.Empty;
            if (imageUrl.Contains('?'))
            {
                var imageBaseUrl = imageUrl.Substring(0, imageUrl.LastIndexOf("?"));
                primaryImage = string.Concat(imageBaseUrl, "?class=normal");
                thumbnailImage = string.Concat(imageBaseUrl, "?class=thumbnail");
            }
            else if (imageUrl.EndsWith("cover-n.jpg"))
            {
                var imageBaseUrl = imageUrl.Substring(0, imageUrl.LastIndexOf("cover-n"));
                primaryImage = string.Concat(imageBaseUrl, "cover-n.jpg");
                thumbnailImage = string.Concat(imageBaseUrl, "cover-t.jpg");
            }
            if (string.IsNullOrEmpty(primaryImage)) return false;
            meta.AddImage(new Models.Info.ItemImageInfo
            {
                Type = ImageType.Primary,
                Path = primaryImage,
            });
            meta.AddImage(new Models.Info.ItemImageInfo
            {
                Type = ImageType.Thumb,
                Path = thumbnailImage,
            });
            return true;
        }

        private static string GetContent(string text)
        {
            var startIndex = text.IndexOf(":") + 1;
            return text.Substring(startIndex).Trim('\0', ' ', '\n', '\t');
        }

        private static string GetRealActor(string text)
        {
            if (text.Contains('('))
            {
                var startIndex = text.IndexOf("(") + 1;
                var endIndex = text.IndexOf(")");
                return text[startIndex..endIndex].Trim('\0', ' ', '\n', '\t');
            }
            return text;

        }
    }
}
