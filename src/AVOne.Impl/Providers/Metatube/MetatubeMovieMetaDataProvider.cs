// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Metatube
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Configuration;
    using AVOne.Constants;
    using AVOne.Impl.Constants;
    using AVOne.Models.Result;
    using AVOne.Providers;
    using PornMovieInfo = AVOne.Models.Info.PornMovieInfo;
    using PornMovie = AVOne.Models.Item.PornMovie;

    public class MetatubeMovieMetaDataProvider : IRemoteMetadataProvider<PornMovie, PornMovieInfo>
    {
        private readonly MetatubeApiClient _client;

        public MetatubeMovieMetaDataProvider(IOfficialProvidersConfiguration officialProvidersConfiguration, IHttpClientFactory httpClientFactory)
        {
            _client = new MetatubeApiClient(httpClientFactory.CreateClient(HttpClientNames.MetatubeClient), officialProvidersConfiguration.MetaTube);
        }
        public string Name => OfficialProviderNames.MetaTube;

        public Task<MetadataResult<PornMovie>> GetMetadata(PornMovieInfo info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<RemoteMetadataSearchResult>> GetSearchResults(PornMovieInfo searchInfo, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
