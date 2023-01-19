// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Metadata
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Configuration;
    using AVOne.Constants;
    using AVOne.Impl.Constants;
    using AVOne.Impl.Providers.Metadata.Metatube;
    using AVOne.Models.Item;
    using AVOne.Models.Result;
    using AVOne.Providers;
    using MovieInfo = Models.Info.MovieInfo;

    public class MetatubeMovieMetaDataProvider : IRemoteMetadataProvider<Movie, MovieInfo>
    {
        private readonly ApiClient _client;

        public MetatubeMovieMetaDataProvider(IOfficialProvidersConfiguration officialProvidersConfiguration, IHttpClientFactory httpClientFactory)
        {
            this._client = new ApiClient(httpClientFactory.CreateClient(HttpClientNames.MetatubeClient), officialProvidersConfiguration.MetaTube);
        }
        public string Name => OfficialProviderNames.MetaTube;

        public Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<RemoteMetadataSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
