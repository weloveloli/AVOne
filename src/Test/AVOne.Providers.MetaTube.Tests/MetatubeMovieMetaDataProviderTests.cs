// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Test.Providers.Metatube
{
    using AutoFixture;
    using AVOne.Providers.Metatube;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using AVOne.Configuration;
    using AVOne.Models.Item;

    public class MetatubeMovieMetaDataProviderTests : BaseTestCase
    {
        private readonly MetatubeMovieMetaDataProvider _provider;
        private readonly TestApplicationConfigs config;
        private readonly string metaTubeServerUrl;
        private readonly bool disableHttpTest;

        public MetatubeMovieMetaDataProviderTests()
        {
            metaTubeServerUrl = Environment.GetEnvironmentVariable("MetaTubeServerUrl");
            disableHttpTest = bool.Parse(Environment.GetEnvironmentVariable("disableHttpTest") ?? "false");
            config = new TestApplicationConfigs();
            config.MetaTube.Server = metaTubeServerUrl;
            var mockManager = fixture.Freeze<Mock<IConfigurationManager>>();
            mockManager.Setup(m => m.CommonConfiguration).Returns(config);
            BaseItem.ConfigurationManager = mockManager.Object;
            var logMock = fixture.Freeze<Mock<ILogger<MetatubeMovieMetaDataProvider>>>();
            fixture.Register((IConfigurationManager manager) => new MetatubeApiClient(new HttpClient(), manager));
            _provider = fixture.Build<MetatubeMovieMetaDataProvider>().Create();
        }

        [SkippableFact]
        public async Task GetMetadataTest()
        {
            Skip.If(string.IsNullOrEmpty(metaTubeServerUrl) || disableHttpTest);
            var porn = new PornMovie
            {
                Name = "stars-507"
            };
            var data = await _provider.GetMetadata(porn.PornMovieInfo, default);
            Assert.NotNull(data);

        }

        [SkippableFact]
        public async Task GetSearchResultsTest()
        {
            Skip.If(string.IsNullOrEmpty(metaTubeServerUrl) || disableHttpTest);
            var porn = new PornMovie
            {
                Name = "stars-507"
            };
            var data = await _provider.GetSearchResults(porn.PornMovieInfo, default);

            Assert.NotNull(data);
            Assert.True(data.Count() >= 1);
        }
    }
}
