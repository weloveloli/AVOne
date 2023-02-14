// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Test.Providers.Metatube
{
    using AutoFixture;
    using AVOne.Impl.Configuration;
    using AVOne.Providers.Metatube;
    using AVOne.Models.Info;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

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
            fixture.Register<IOfficialProvidersConfiguration>(() => config);
            var logMock = fixture.Freeze<Mock<ILogger<MetatubeMovieMetaDataProvider>>>();
            fixture.Register(() => new MetatubeApiClient(new HttpClient(), config));
            _provider = fixture.Build<MetatubeMovieMetaDataProvider>().Create();
        }

        [SkippableFact]
        public async Task GetMetadataTest()
        {
            Skip.If(string.IsNullOrEmpty(metaTubeServerUrl) || disableHttpTest);
            var data = await _provider.GetMetadata(new PornMovieInfo
            {
                Name = "stars-507"
            }, default);
            Assert.NotNull(data);

        }

        [SkippableFact]
        public async Task GetSearchResultsTest()
        {
            Skip.If(string.IsNullOrEmpty(metaTubeServerUrl) || disableHttpTest);
            var data = await _provider.GetSearchResults(new PornMovieInfo
            {
                Name = "stars-507"
            }, default);

            Assert.NotNull(data);
            Assert.True(data.Count() >= 1);
        }
    }
}
