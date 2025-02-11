// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable
namespace AVOne.Plugins.MetaTube
{
    using AVOne.Common.Enum;
    using AVOne.Plugins.MetaTube.Configuration;
    using Furion.FriendlyException;
    using Microsoft.Extensions.Logging;

    public abstract class BaseProvider
    {
        protected readonly ILogger Logger;
        protected readonly MetatubeApiClient ApiClient;
        protected MetaTubeConfiguration Configuration => Plugin.Instance.Configuration;

        protected BaseProvider(ILogger logger, MetatubeApiClient metatubeApiClient)
        {

            Logger = logger;
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
