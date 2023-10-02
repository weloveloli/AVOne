// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Plugins.Extractors
{
    using AVOne.Abstraction;
    using AVOne.Providers.Official.Common;
    using Microsoft.Extensions.DependencyInjection;

    public class OfficialPluginRegistrator : IServiceRegistrator
    {
        public void PostBuildService(IApplicationHost host)
        {
        }

        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IHttpHelper, DefaultHttpHelper>();
        }
    }
}
