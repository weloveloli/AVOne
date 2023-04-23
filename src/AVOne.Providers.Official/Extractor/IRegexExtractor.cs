// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor
{
    public interface IRegexExtractor
    {
        public IEnumerable<string> GetM3U8Sources(string html);
        public string GetTitleFromHtml(string html);
    }
}
