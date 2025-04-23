// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Tests.Extractor
{
    using AVOne.Models.Download;
    using AVOne.Providers.Official.Common;
    using AVOne.Providers.Official.Extractors;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class XChinaExtractorTests
    {
        [Fact()]
        public async Task XChinaExtractorTest()
        {
            var html = File.ReadAllText(Path.Combine("websites", "xchina.txt"));
            var loggerMock = new Mock<ILogger<XChinaExtractor>>();
            var helperMock = new Mock<IHttpHelper>();
            helperMock.Setup(x => x.GetHtmlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(html);
            helperMock.Setup(x => x.DownloadThumbAsync(It.Is<string>(e => e == "https://upload.xchina.store/video/6649fc556f8a1.webp"), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>())).ReturnsAsync("localfile");
            var extractor = new XChinaExtractor(helperMock.Object, loggerMock.Object);
            var source = await extractor.ExtractAsync(html);
            Assert.Single(source);

            var item = source.First();
            Assert.IsType<M3U8Item>(item);
            var m3u8Item = item as M3U8Item;

            Assert.Equal("https://s1.playhls.com/m3u8.php?s=emNSRkdzUzkyTE5LSkVaRzhQVmduMFJpU1U5YVoyNWxjMWxuZVVOMmRWWXlOSE1yZGxreWQzUnNaVkpQTlZkNE9FdGFSbWRDVURWTmNrWm9UMWR6TjNSR1VTdFFNV1pHWTFwbmJITnJkVlZaZDFoNGVISXZVMmw2VFdocWFGaFBXWHAzWjIxVWNWaERkMlF6VlVSaFJ6WkxZa2xJZWt4d05YQjNQUT09", m3u8Item.Url);
            Assert.Equal(Enum.MediaQuality.High, m3u8Item.Quality);
            Assert.Equal("清纯女主播主动献身榜一大哥胯下", m3u8Item.Title);
            Assert.Equal("XKG197", m3u8Item.SaveName);
            Assert.Single(m3u8Item.People);
            Assert.Equal("沈佳吟", m3u8Item.People.First().Name);

            Assert.Single(m3u8Item.ImageInfos);
            Assert.Equal("localfile", m3u8Item.ImageInfos.First().Path);
        }
    }
}
