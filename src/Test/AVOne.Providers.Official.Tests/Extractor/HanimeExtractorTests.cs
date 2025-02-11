// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Tests
{
    using AVOne.Providers.Official.Common;
    using AVOne.Providers.Official.Extractors;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class HanimeExtractorTests
    {
        private readonly HanimeExtractor extractor;
        private readonly HtmlNode domNode;

        public HanimeExtractorTests()
        {
            var httpHelper = new Mock<IHttpHelper>();
            var loggerMock = new Mock<ILoggerFactory>();
            this.extractor = new HanimeExtractor(httpHelper.Object, loggerMock.Object);
            var html = File.ReadAllText(Path.Combine("websites", "hanime1.txt"));
            var dom = new HtmlDocument();
            dom.LoadHtml(html);
            domNode = dom.DocumentNode;

        }
        [Fact()]

        public void SupportTest()
        {
            Assert.True(extractor.Support("https://hanime1.me/watch?v=39137"));
        }

        [Fact()]
        public void GetM3U8SourcesTest()
        {
            var sources = extractor.GetSources(domNode);
            Assert.Single(sources, "https://vstream.hembed.com/hls/39686-sc.m3u8?token=5FaPI_sSD8zIFeYd1ZFZoxwWMewL1HgsIOPt4DlCDZQ&expires=1682254805");
        }

        [Fact()]
        public void GetTitleTest()
        {
            var title = extractor.GetTitle(domNode);
            Assert.Equal("[アトリエこぶ] ザナルカンドにて [中文字幕]", title);
        }
    }
}
