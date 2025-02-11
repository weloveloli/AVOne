// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractors.Base
{
    using AVOne.Models.Download;
    using HtmlAgilityPack;

    public interface IDOMExtractor
    {
        /// <summary>
        /// Get the title of the page
        /// </summary>
        /// <param name="dom"></param>
        /// <returns></returns>
        public string GetTitle(HtmlNode dom);

        /// <summary>
        /// Get the items from the page
        /// </summary>
        /// <param name="title"></param>
        /// <param name="node"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public IEnumerable<BaseDownloadableItem> GetItems(string title, HtmlNode node, string url);
    }
}
