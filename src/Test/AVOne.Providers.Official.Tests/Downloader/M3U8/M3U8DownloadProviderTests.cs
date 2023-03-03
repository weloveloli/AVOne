// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.M3U8.Tests
{
    using Xunit;
    using AVOne.Providers.Official.Downloader.M3U8;
    using AVOne.Test.Base;
    using AutoFixture;
    using AVOne.Configuration;
    using Moq;
    using AVOne.Constants;
    using AVOne.Models.Download;
    using AVOne.Common.Helper;

    public class M3U8DownloadProviderTests : BaseTestCase
    {
        private M3U8DownloadProvider _provider;
        public M3U8DownloadProviderTests()
        {
            var cacheDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "AVOneTool", "cache");
            var configMock = fixture.Freeze<Mock<IApplicationPaths>>();
            configMock.Setup(c => c.CachePath).Returns(cacheDir);

            var mockStartUp = fixture.Freeze<Mock<IStartupOptions>>();

            //mockStartUp.Setup(mock => mock.FFmpegPath).Returns(@"C:\ffmpeg\bin\ffmpeg.exe");
            mockStartUp.Setup(mock => mock.FFmpegPath).Returns(ExecutableHelper.FindExecutable("ffmpeg"));

            var mockHttpClientFactory = fixture.Freeze<Mock<IHttpClientFactory>>();

            mockHttpClientFactory.Setup(e => e.CreateClient(AVOneConstants.Download)).Returns(new HttpClient());

            _provider = fixture.Create<M3U8DownloadProvider>();
        }

        [SkippableFact]
        public async Task CreateTaskTest()
        {
            var ffmpeg = ExecutableHelper.FindExecutable("ffmpeg");
            Skip.If(string.IsNullOrEmpty(ffmpeg));
            var opts = new DownloadOpts
            {
                ThreadCount = 4,
            };
            opts.StatusChanged += (o, e) => Console.WriteLine(e.Status);
            await _provider.CreateTask(new M3U8Item(
                "test",
                //  "https://cdn52.akamai-content-network.com/bcdn_token=q7FuajXU_tS6Wre3upBJ6u-Aenbm8I5Rqq3OfEzj6YE&expires=1677903595&token_path=%2Fe23d2212-70ca-478c-8691-654dddd58c2d%2F/e23d2212-70ca-478c-8691-654dddd58c2d/1280x720/video.m3u8",
                "https://cdn52.akamai-content-network.com/bcdn_token=TCuEwbR4Xuwyp_A1O5Qcy9rFba2H3_iDcYFkoRGrY5c&expires=1677911766&token_path=%2Fe23d2212-70ca-478c-8691-654dddd58c2d%2F/e23d2212-70ca-478c-8691-654dddd58c2d/1280x720/video.m3u8",
                 new Dictionary<string, string> {
                { "referer", "https://missav.com" },
                { "origin", "https://missav.com" }
            },
                 Enum.MediaQuality.Medium,
                 "test"
                ), opts, default);
        }

        [Fact()]
        public void SupportTest()
        {

        }
    }
}
