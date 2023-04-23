// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Base
{
    using AVOne.Models.Download;

    public interface IRegexExtractor
    {
        public IEnumerable<BaseDownloadableItem> GetItems(string title, string html, string url);
        public string GetTitle(string html);
    }
}
