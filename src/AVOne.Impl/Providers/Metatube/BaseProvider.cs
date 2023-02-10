// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Metatube
{
    using AVOne.Configuration;
    using AVOne.Impl.Constants;
    using Microsoft.Extensions.Logging;

    public abstract class BaseProvider
    {
        protected readonly ILogger Logger;
        protected readonly MetatubeApiClient ApiClient;
        protected readonly IOfficialProvidersConfiguration OfficialProvidersConfiguration;

        protected BaseProvider(ILogger logger, IOfficialProvidersConfiguration officialProvidersConfiguration, MetatubeApiClient metatubeApiClient)
        {
            Logger = logger;
            ApiClient = metatubeApiClient;
            OfficialProvidersConfiguration = officialProvidersConfiguration;
        }
        protected MetaTubeConfiguration Configuration => OfficialProvidersConfiguration.MetaTube;

        public virtual int Order => 1;

        public virtual string Name => OfficialProviderNames.MetaTube;

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            Logger.LogDebug("GetImageResponse for url: {0}", url);
            return ApiClient.GetImageResponse(url, cancellationToken);
        }
    }
}
