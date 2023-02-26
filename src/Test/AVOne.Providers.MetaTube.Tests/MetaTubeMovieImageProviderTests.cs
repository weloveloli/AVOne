// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.MetaTube.Tests
{
    using AutoFixture;
    using AVOne.Configuration;
    using AVOne.Models.Item;
    using AVOne.Providers.Metatube;
    using AVOne.Providers.MetaTube.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class MetaTubeMovieImageProviderTests : BaseTestCase
    {
        private readonly MetaTubeConfiguration config;
        private readonly string metaTubeServerUrl;
        private readonly bool disableHttpTest;

        public MetaTubeMovieImageProviderTests()
        {
            metaTubeServerUrl = Environment.GetEnvironmentVariable("MetaTubeServerUrl");
            disableHttpTest = bool.Parse(Environment.GetEnvironmentVariable("disableHttpTest") ?? "false");
            var testConfig = new TestApplicationConfigs();
            config = new MetaTubeConfiguration();
            config.Server = metaTubeServerUrl;
            var mockManager = fixture.Freeze<Mock<IConfigurationManager>>();
            mockManager.Setup(m => m.CommonConfiguration).Returns(testConfig);
            var logMock = fixture.Freeze<Mock<ILogger<MetatubeMovieMetaDataProvider>>>();
            fixture.Register((IConfigurationManager manager) => new MetatubeApiClient(new HttpClient(), manager));
        }

        [SkippableFact(typeof(NotSupportedException), typeof(NotImplementedException))]
        public async Task MetaTubeMovieImageProviderTest()
        {
            Skip.If(string.IsNullOrEmpty(metaTubeServerUrl) || disableHttpTest);
            var _provider = fixture.Build<MetaTubeMovieImageProvider>().Create();
            var data = await _provider.GetImages(new PornMovie
            {
                Name = "stars-507",
                ProviderIds = new Dictionary<string, string>
                {
                    { "MetaTube","AVWIKI:STARS-507"}
                }

            }, default);

            Assert.NotNull(data);
            Assert.True(data.Count() >= 1);
        }
    }
}
