// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Extensions
{
    using System;
    using System.Collections.Generic;
    using AVOne.Abstraction;

    /// <summary>
    /// Class ProviderIdsExtensions.
    /// </summary>
    public static class ProviderIdsExtensions
    {
        /// <summary>
        /// Sets a provider id.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public static void SetProviderId(this IHasProviderIds instance, string name, string? value)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            // If it's null remove the key from the dictionary
            if (string.IsNullOrEmpty(value))
            {
                _ = (instance.ProviderIds?.Remove(name));
            }
            else
            {
                // Ensure it exists
                instance.ProviderIds ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                // Match on internal MetadataProvider enum string values before adding arbitrary providers
                instance.ProviderIds[name] = value;
            }
        }
    }
}
