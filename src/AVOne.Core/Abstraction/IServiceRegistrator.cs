// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Abstraction
{
    using Microsoft.Extensions.DependencyInjection;

    public interface IServiceRegistrator
    {
        /// <summary>
        /// Registers the module or plugin's services with the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        void RegisterServices(IServiceCollection serviceCollection);

        void RegisterStatic(IApplicationHost host);
    }
}
