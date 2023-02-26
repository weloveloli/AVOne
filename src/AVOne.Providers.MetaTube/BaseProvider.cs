// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable
namespace AVOne.Providers.Metatube
{
    using AVOne.Common.Enum;
    using AVOne.Configuration;
    using AVOne.Providers.MetaTube.Configuration;
    using Furion.FriendlyException;
    using Microsoft.Extensions.Logging;

    public abstract class BaseProvider
    {
        protected readonly ILogger Logger;
        protected readonly IConfigurationManager _configurationManager;
        protected readonly MetatubeApiClient ApiClient;
        protected MetaTubeConfiguration Configuration => _configurationManager.GetConfiguration<MetaTubeConfiguration>(MetaTubeConfigStore.StoreKey);

        protected BaseProvider(ILogger logger, IConfigurationManager configurationManager, MetatubeApiClient metatubeApiClient)
        {

            Logger = logger;
            _configurationManager = configurationManager;
            ApiClient = metatubeApiClient;
        }

        public virtual int Order => 1;

        public virtual string Name => "MetaTube";

        public virtual bool IsProviderAvailable()
        {
            return !string.IsNullOrEmpty(Configuration.Server);
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            if (!IsProviderAvailable())
            {
                throw Oops.Oh(ErrorCodes.PROVIDER_NOT_AVAILABLE, Name);
            }
            Logger.LogDebug("GetImageResponse for url: {0}", url);
            return ApiClient.GetImageResponse(url, cancellationToken);
        }
    }
}
