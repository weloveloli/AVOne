// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    using AVOne.Abstraction;
    using Microsoft.Extensions.DependencyInjection;

    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddProvider<TProvider, TProviderImplementation>(this IServiceCollection services)
            where TProvider : IProvider
            where TProviderImplementation : class, TProvider
        {
            var providerType = typeof(TProvider);
            var providerImplementationType = typeof(TProviderImplementation);
            services.TryAdd(ServiceDescriptor.Singleton(providerType, providerImplementationType));
            return services;
        }
    }
}
