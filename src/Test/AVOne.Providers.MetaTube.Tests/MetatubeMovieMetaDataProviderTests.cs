// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Test.Providers.Metatube
{
    using AutoFixture;
    using AVOne.Configuration;
    using AVOne.Models.Item;
    using AVOne.Providers.Metatube;
    using AVOne.Providers.MetaTube.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class MetatubeMovieMetaDataProviderTests : BaseTestCase
    {
        private readonly TestApplicationConfigs config;
        private readonly string metaTubeServerUrl;
        private readonly bool disableHttpTest;

        public MetatubeMovieMetaDataProviderTests()
        {
            metaTubeServerUrl = Environment.GetEnvironmentVariable("MetaTubeServerUrl");
            disableHttpTest = bool.Parse(Environment.GetEnvironmentVariable("disableHttpTest") ?? "false");
            config = new TestApplicationConfigs();
            var testConfig = new TestApplicationConfigs();
            var metaconfig = new MetaTubeConfiguration();
            metaconfig.Server = metaTubeServerUrl;
            var mockManager = fixture.Freeze<Mock<IConfigurationManager>>();
            mockManager.Setup(m => m.CommonConfiguration).Returns(config);
            BaseItem.ConfigurationManager = mockManager.Object;
            var logMock = fixture.Freeze<Mock<ILogger<MetatubeMovieMetaDataProvider>>>();
            fixture.Register((IConfigurationManager manager) => new MetatubeApiClient(new HttpClient(), manager));

        }

        [SkippableFact]
        public async Task GetMetadataTest()
        {
            Skip.If(string.IsNullOrEmpty(metaTubeServerUrl) || disableHttpTest);
            var provider = fixture.Build<MetatubeMovieMetaDataProvider>().Create();
            var porn = new PornMovie
            {
                Name = "stars-507"
            };
            var data = await provider.GetMetadata(porn.PornMovieInfo, default);
            Assert.NotNull(data);

        }

        [SkippableFact]
        public async Task GetSearchResultsTest()
        {
            Skip.If(string.IsNullOrEmpty(metaTubeServerUrl) || disableHttpTest);
            var provider = fixture.Build<MetatubeMovieMetaDataProvider>().Create();
            var porn = new PornMovie
            {
                Name = "stars-507"
            };
            var data = await provider.GetSearchResults(porn.PornMovieInfo, default);

            Assert.NotNull(data);
            Assert.True(data.Count() >= 1);
        }
    }
}
