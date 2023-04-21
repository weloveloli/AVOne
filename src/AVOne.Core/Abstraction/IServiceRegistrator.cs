// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Abstraction
{
    using Microsoft.Extensions.DependencyInjection;

    public interface IServiceRegistrator
    {
        /// <summary>
        /// Registers the module or plugin's services with the service collection.
        /// </summary>
        /// <param name="service">The service collection.</param>
        void RegisterServices(IServiceCollection service);

        void PostBuildService(IApplicationHost host);
    }
}
