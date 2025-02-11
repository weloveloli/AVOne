// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor
{
    using System.Collections.Generic;
    using AVOne.Providers.Official.Extractor.Base;
    using AVOne.Providers.Official.Extractor.Embeded;
    using Microsoft.Extensions.Logging;

    public class DnaavExtrator : BaseEmbedHttpExtractor
    {
        public DnaavExtrator(IHttpHelper httpHelper, ILoggerFactory loggerFactory)
            : base(httpHelper, loggerFactory, "https://www.dnaav.com", new EmbedDnaavExtrator())
        {
        }

        public override string Name => "Dnaav";

        public override IEnumerable<string> GetEmbedPages(string url, string html)
        {
            // from the current url https://www.dnaav.com/video/215963.html we can get the embed url https://www.dnaav.com/embed/215963.html
            var embedUrl = url.Replace("video", "embed");
            return new List<string> { embedUrl };
        }
    }
}
