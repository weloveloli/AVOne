// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Tests
{
    using AVOne.Models.Download;
    using AVOne.Providers.Official.Common;
    using AVOne.Providers.Official.Extractors;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class TaiavExtratorTests
    {
        [Fact()]
        public async Task TaiavExtratorTest()
        {
            var html = File.ReadAllText(Path.Combine("websites", "taimadou.txt"));
            var json = File.ReadAllText(Path.Combine("websites", "taimadou_api.txt"));
            var loggerMock = new Mock<ILoggerFactory>();
            var mockHttpHelper = new Mock<IHttpHelper>();
            var url = "https://taimadou.com/cn/movie/653d8ebfdb328a26584f66a5";
            var apiUrl = "https://taimadou.com/api/getmovie?type=1280&id=653d8ebfdb328a26584f66a5";

            mockHttpHelper.Setup(m => m.GetHtmlAsync(It.Is<string>(e => e == url), It.IsAny<CancellationToken>()))
                .ReturnsAsync(html);
            mockHttpHelper.Setup(m => m.GetHtmlAsync(It.Is<string>(e => e == apiUrl), It.IsAny<CancellationToken>()))
                .ReturnsAsync(json);
            var extrator = new TaiavExtrator(mockHttpHelper.Object, loggerMock.Object);
            var items = await extrator.ExtractAsync(url, CancellationToken.None);
            Assert.Single(items);

            var item = items.First();
            Assert.IsType<M3U8Item>(item);
            var m3u8Item = item as M3U8Item;
            Assert.Equal("https://taimadou.com/videos/202310/29/653d8ebfdb328a26584f66a5/ddg547/index.m3u8?random=72d653c0aF6D0dAdf90F", m3u8Item.Url);
            Assert.Equal(Enum.MediaQuality.Medium, m3u8Item.Quality);
            Assert.Equal("糖心vlog 边洗边爱浴室爱爱vlog 辛尤里", m3u8Item.Name);
        }
    }
}

