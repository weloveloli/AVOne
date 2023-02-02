// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Official
{
    using AVOne.Configuration;
    using AVOne.Impl.Constants;
    using AVOne.Providers;

    public class OfficialMovieNameParserV2Provider : IPornMovieNameParserProvider
    {
        private readonly IApplicationConfigs _configs;

        public int Order => 1;

        public string Name => OfficialProviderNames.Official;

        public OfficialMovieNameParserV2Provider(IApplicationConfigs configs)
        {
            this._configs = configs;
        }

        public MovieId Parse(string movieName)
        {
            
        }
    }
}
