// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor
{
    using System.Collections.Generic;
    using System.Net.Http;
    using AVOne.Configuration;
    using Fizzler.Systems.HtmlAgilityPack;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class HanimeExtractor : BaseHttpExtractor, IDOMExtractor
    {
        public HanimeExtractor(IConfigurationManager manager, ILogger<HanimeExtractor> logger, IHttpClientFactory httpClientFactory) : base(manager, logger, httpClientFactory, "https://hanime1.me")
        {
        }

        public override string Name => "hanime";

        public IEnumerable<string> GetM3U8Sources(HtmlNode dom)
        {
            var content = dom.QuerySelector("script[type='application/ld+json']").InnerHtml;
            var VideoObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            yield return VideoObject["contentUrl"].ToString()!;
        }

        public string GetTitle(HtmlNode dom)
        {
            var content = dom.QuerySelector("script[type='application/ld+json']").InnerHtml;
            var VideoObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            return VideoObject["name"].ToString()!;
        }
    }
}
