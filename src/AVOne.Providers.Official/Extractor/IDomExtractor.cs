// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor
{
    using HtmlAgilityPack;

    public interface IDOMExtractor
    {
        public IEnumerable<string> GetM3U8Sources(HtmlNode dom);
        public string GetTitle(HtmlNode dom);
    }
}
