// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Metatube
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Configuration;
    using AVOne.Models.Result;
    using AVOne.Providers;
    using Microsoft.Extensions.Logging;
    using System.Net.Http;
    using AVOne.Models.Item;
    using AVOne.Models.Info;

    public class MetatubeMovieMetaDataProvider : BaseProvider, IRemoteMetadataProvider<PornMovie, PornMovieInfo>
    {
        public MetatubeMovieMetaDataProvider(ILoggerFactory loggerFactory,
                                             IOfficialProvidersConfiguration officialProvidersConfiguration,
                                             IHttpClientFactory httpClientFactory)
            : base(loggerFactory, officialProvidersConfiguration, httpClientFactory)
        {
        }

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
