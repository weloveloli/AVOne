// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    using AVOne.Abstraction;

    public static class IServiceProviderExtensions
    {
        public static TProvider GetProvider<TProvider>(this IServiceProvider provider, string providerName) where TProvider : IProvider
        {
            var providers = provider.GetServices(typeof(TProvider)) as IEnumerable<TProvider>;
            return providers.FirstOrDefault(e => e.Name == providerName);
        }
    }
}
