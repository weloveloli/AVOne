// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Extensions
{
#nullable disable

    using AVOne.Extensions;
    using AVOne.Providers;
    using Microsoft.Extensions.DependencyInjection;

    public static class IServiceProviderExtensions
    {
        public static TProvider GetProvider<TProvider>(this IServiceProvider provider, string providerName) where TProvider : IProvider
        {
            var providers = provider.GetServices(typeof(TProvider)) as IEnumerable<TProvider>;
            return providers.FirstOrDefault(e => e.Name == providerName);
        }
    }
}
