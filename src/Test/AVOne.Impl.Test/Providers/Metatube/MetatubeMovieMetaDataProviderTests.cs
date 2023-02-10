// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Metatube.Tests
{
    using Xunit;
    using AVOne.Impl.Providers.Metatube;
    using AVOne.Impl.Test;
    using AutoFixture;
    using AVOne.Models.Info;
    using AVOne.Configuration;
    using Moq;
    using Microsoft.Extensions.Logging;

    public class MetatubeMovieMetaDataProviderTests : BaseTestCase
    {
        private readonly MetatubeMovieMetaDataProvider _provider;
        private TestApplicationConfigs config;
        private string? metaTubeServerUrl;
        private bool disableHttpTest;

        public MetatubeMovieMetaDataProviderTests()
        {
            this.metaTubeServerUrl = Environment.GetEnvironmentVariable("MetaTubeServerUrl");
            this.disableHttpTest = bool.Parse(Environment.GetEnvironmentVariable("disableHttpTest") ?? "false");
            this.config = new TestApplicationConfigs();
            this.config.MetaTube.Server = metaTubeServerUrl;
            fixture.Register<IOfficialProvidersConfiguration>(() => this.config);
            var logMock = fixture.Freeze<Mock<ILogger<MetatubeMovieMetaDataProvider>>>();
            fixture.Register(() => new MetatubeApiClient(new HttpClient(), this.config));
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

        [Fact()]
        public void GetSearchResultsTest()
        {
            Assert.True(false, "This test needs an implementation");
        }
    }
}
