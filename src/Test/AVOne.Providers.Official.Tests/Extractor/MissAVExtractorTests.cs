// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Tests
{
    using AVOne.Providers.Official.Extractors;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class MissAVExtractorTests
    {
        [Fact()]
        public void MissAVExtractorTest()
        {

            var loggerMock = new Mock<ILoggerFactory>();
            var html = File.ReadAllText(Path.Combine("websites", "missav.txt"));
            var extractor = new MissAVExtractor(null, loggerMock.Object);
            var support = extractor.Support("https://missav.ws/ja/fc2-ppv-4525803");

            Assert.True(support);

            var source = extractor.GetM3U8Sources(html);

            Assert.Equal(3, source.Count());

            Assert.Equal("CUS-1468 變態boss捆綁調教新人女員工", MissAVExtractor.GetTitleFromHtml(html, MissAVExtractor.TitleRegex()));
        }
    }
}
