// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Test
{
    using AVOne.Configuration;
    using AVOne.Impl.Providers.Metatube;
    using Xunit;

    public class TestMetatube
    {
        [SkippableFact]
        public async Task Test1()
        {
            var metaTubeServerUrl = Environment.GetEnvironmentVariable("MetaTubeServerUrl");
            Skip.IfNot(!string.IsNullOrEmpty(metaTubeServerUrl));
            var client = new MetatubeApiClient(new HttpClient(), new MetaTubeConfiguration
            {
                Server = metaTubeServerUrl,
            });
            var data = await client.SearchMovieAsync("TDMN-009", default);

            Assert.NotNull(data);

        }
    }
}
