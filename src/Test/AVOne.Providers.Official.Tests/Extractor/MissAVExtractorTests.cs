// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Tests
{
    using Xunit;
    using AVOne.Providers.Official.Extractor;

    public class MissAVExtractorTests
    {
        [Fact()]
        public void MissAVExtractorTest()
        {
            var html = File.ReadAllText(Path.Combine("websites", "missav.html"));

            var source = new MissAVExtractor(null, null).GetSources(html);

            Assert.Equal(3, source.Count());

            Assert.Equal("CUS-1468 變態boss捆綁調教新人女員工", MissAVExtractor.GetTitleFromHtml(html, MissAVExtractor.GetTitleRegex()));
        }
    }
}
