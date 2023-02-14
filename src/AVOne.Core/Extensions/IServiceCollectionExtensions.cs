// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Extensions
{
#nullable disable

    using AVOne.Providers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

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
