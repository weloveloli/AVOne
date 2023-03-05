// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Tests
{
    using Xunit;
    using AVOne.Providers.Official.Extractor;
    using Moq;

    public class AVPLEAVExtractorTests
    {
        [Fact()]
        public void Tests()
        {
            var html = File.ReadAllText(Path.Combine("websites", "avple.txt"));

            var source = new AvpleTvExtractor(new Mock<IHttpClientFactory>().Object, null).GetSources(html);

            Assert.Single(source);

            Assert.Equal("烏拖邦WTB066忍無可忍的兄妹教育", AvpleTvExtractor.GetTitleFromHtml(html, AvpleTvExtractor.TitleRegex()));
        }
    }
}
