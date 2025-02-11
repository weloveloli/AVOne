// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official
{
    using AVOne.Abstraction;
    using AVOne.Providers.Official.Extractor;
    using AVOne.Providers.Official.Extractor.Base;
    using Microsoft.Extensions.DependencyInjection;

    public class OfficialServiceRegistrator : IServiceRegistrator
    {
        public void PostBuildService(IApplicationHost host)
        {
        }

        public void RegisterServices(IServiceCollection service)
        {
            service.AddSingleton<IHttpHelper, DefaultHttpHelper>();
        }
    }
}
