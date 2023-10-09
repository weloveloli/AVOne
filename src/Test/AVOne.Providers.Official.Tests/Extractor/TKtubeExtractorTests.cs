// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Tests.Extractor
{
    using AutoFixture;
    using AVOne.Providers.Official.Common;
    using AVOne.Providers.Official.Extractors;
    using AVOne.Providers.Official.Extractors.Embeded;
    using AVOne.Test.Base;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class TKtubeExtractorTests : BaseTestCase
    {
        [Fact]
        public async Task TKtubeExtractorTest()
        {
            var lf = fixture.Freeze<Mock<ILoggerFactory>>();
            lf.Setup(e => e.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

            var httpHelper = fixture.Freeze<Mock<IHttpHelper>>();
            _ = httpHelper.Setup(
                 e => e.GetHtmlAsync(It.Is<string>((url) => url == "https://tktube.com/videos/121939/1854/"),
                 It.IsAny<CancellationToken>()))
                .ReturnsAsync(File.ReadAllText(Path.Combine("websites", "tktube.txt")));

            _ = httpHelper.Setup(
                e => e.GetHtmlAsync(It.Is<string>((url) => url == "https://tktube.com/embed/121939"),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(File.ReadAllText(Path.Combine("websites", "tktubeembed.txt")));
            //fixture.Register((ILoggerFactory lf, IHttpHelper httpHelper) => new TKtubeExtractor(httpHelper, lf));

            var extractor = fixture.Create<TKtubeExtractor>();
            var items = await extractor.ExtractAsync("https://tktube.com/videos/121939/1854/", default);

            Assert.True(items.Any());
            Assert.Equal(3, items.Count());

            Assert.True(items.All(e => e.OrignalLink.Equals("https://tktube.com/videos/121939/1854/")));
            Assert.True(items.All(e => e.Genres.Any()));
            Assert.True(items.All(e => e.Tags.Any()));
            Assert.True(items.All(e => e.HasMetaData));
            Assert.True(items.All(e => e.HomePageUrl.Equals("https://tktube.com/videos/121939/1854/")));
        }

        [Theory]
        [InlineData("$432515114269431", "54364362706040403733399244753648")]
        public void TestGetCode(string input, string expect)
        {
            Assert.Equal(expect, TKtubeEmbededExtractorUtils.GetCode(input));
        }

        [Theory]
        [InlineData("function/5376/https://tktube.com/get_file/24/a1f751d70e200f51a77b8d12b9066ffdaeb230246b/121000/121938/121938_720p.mp4/?embed=true", "$432515114269431", "https://tktube.com/get_file/24/fff167a2bd0751e7a91d81bf02d07065aeb230246b/121000/121938/121938_720p.mp4/?embed=true")]
        [InlineData("function/0/https://tktube.com/get_file/24/f19b35f026e5ddd42ec72a93a0f1b2b4a2d597374e/121000/121939/121939_360p.mp4/?embed=true", "$432515114269431", "https://tktube.com/get_file/24/b2d41c23a42b3960f01f2579deafe5bda2d597374e/121000/121939/121939_360p.mp4/?embed=true")]
        public void TestRealUrl(string input, string license, string expect)
        {
            Assert.Equal(expect, TKtubeEmbededExtractorUtils.GetRealUrl(input, license));
        }
    }
}
