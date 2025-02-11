// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractors.Base
{
    using AVOne.Models.Download;
    using HtmlAgilityPack;

    public interface IDOMExtractor
    {
        public string GetTitle(HtmlNode dom);
        public IEnumerable<BaseDownloadableItem> GetItems(string title, HtmlNode node, string url);
    }
}
