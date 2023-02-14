// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable
namespace AVOne.Providers.Metatube
{
    using AVOne.Configuration;
    using AVOne.Exception;
    using AVOne.Providers.MetaTube.Configuration;
    using Microsoft.Extensions.Logging;

    public abstract class BaseProvider
    {
        protected readonly ILogger Logger;
        private readonly IConfigurationManager _configurationManager;
        protected readonly MetatubeApiClient ApiClient;
        protected IMetaTubeConfiguration OfficialProvidersConfiguration => _configurationManager.CommonConfiguration as IMetaTubeConfiguration;

        protected BaseProvider(ILogger logger, IConfigurationManager configurationManager, MetatubeApiClient metatubeApiClient)
        {
            if (configurationManager.CommonConfiguration is IMetaTubeConfiguration metaTubeConfiguration)
            {
                if (!string.IsNullOrEmpty(metaTubeConfiguration.MetaTube.Server))
                {
                    Logger = logger;
                    _configurationManager = configurationManager;
                    ApiClient = metatubeApiClient;
                }
                else
                {

                    throw new ProviderNotAvaliableException(this.GetType().FullName, $"Metatube init failed, pls update the config file {configurationManager.ApplicationPaths.SystemConfigurationFilePath} or add 'MetaTubeServerUrl' in enviroment variable");
                }
            }
            else
            {
                throw new ProviderNotAvaliableException(this.GetType().FullName, $"No metatube config found.");
            }
        }

        protected MetaTubeConfiguration Configuration => OfficialProvidersConfiguration.MetaTube;

        public virtual int Order => 1;

        public virtual string Name => "MetaTube";

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            Logger.LogDebug("GetImageResponse for url: {0}", url);
            return ApiClient.GetImageResponse(url, cancellationToken);
        }
    }
}
