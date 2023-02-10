// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web;
    using AVOne.Abstraction;
    using AVOne.Impl.Enum;
    using AVOne.Impl.Helper;

    public static class ProviderIdsExtensions
    {
        /// <summary>
        /// Case insensitive dictionary of <see cref="MetadataProvider"/> string representation.
        /// </summary>
        private static readonly Dictionary<string, string> _metadataProviderEnumDictionary =
            Enum.GetValues<MetadataProvider>()
                .ToDictionary(
                    enumValue => enumValue.ToString(),
                    enumValue => enumValue.ToString(),
                    StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if this instance has an id for the given provider.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The of the provider name.</param>
        /// <returns><c>true</c> if a provider id with the given name was found; otherwise <c>false</c>.</returns>
        public static bool HasProviderId(this IHasProviderIds instance, string name)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return instance.TryGetProviderId(name, out _);
        }

        /// <summary>
        /// Checks if this instance has an id for the given provider.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="provider">The provider.</param>
        /// <returns><c>true</c> if a provider id with the given name was found; otherwise <c>false</c>.</returns>
        public static bool HasProviderId(this IHasProviderIds instance, MetadataProvider provider)
        {
            return instance.HasProviderId(provider.ToString());
        }

        /// <summary>
        /// Gets a provider id.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="id">The provider id.</param>
        /// <returns><c>true</c> if a provider id with the given name was found; otherwise <c>false</c>.</returns>
        public static bool TryGetProviderId(this IHasProviderIds instance, string name, [NotNullWhen(true)] out string? id)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (instance.ProviderIds == null)
            {
                id = null;
                return false;
            }

            var foundProviderId = instance.ProviderIds.TryGetValue(name, out id);
            // This occurs when searching with Identify (and possibly in other places)
            if (string.IsNullOrEmpty(id))
            {
                id = null;
                foundProviderId = false;
            }

            return foundProviderId;
        }

        /// <summary>
        /// Gets a provider id.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="id">The provider id.</param>
        /// <returns><c>true</c> if a provider id with the given name was found; otherwise <c>false</c>.</returns>
        public static bool TryGetProviderId(this IHasProviderIds instance, MetadataProvider provider, [NotNullWhen(true)] out string? id)
        {
            return instance.TryGetProviderId(provider.ToString(), out id);
        }

        /// <summary>
        /// Gets a provider id.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        public static string? GetProviderId(this IHasProviderIds instance, string name)
        {
            instance.TryGetProviderId(name, out string? id);
            return id;
        }

        /// <summary>
        /// Gets a provider id.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="provider">The provider.</param>
        /// <returns>System.String.</returns>
        public static string? GetProviderId(this IHasProviderIds instance, MetadataProvider provider)
        {
            return instance.GetProviderId(provider.ToString());
        }

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
                instance.ProviderIds?.Remove(name);
            }
            else
            {
                // Ensure it exists
                instance.ProviderIds ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                // Match on internal MetadataProvider enum string values before adding arbitrary providers
                if (_metadataProviderEnumDictionary.TryGetValue(name, out var enumValue))
                {
                    instance.ProviderIds[enumValue] = value;
                }
                else
                {
                    instance.ProviderIds[name] = value;
                }
            }
        }

        /// <summary>
        /// Sets a provider id.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="value">The value.</param>
        public static void SetProviderId(this IHasProviderIds instance, MetadataProvider provider, string value)
        {
            instance.SetProviderId(provider.ToString(), value);
        }

        public static ProviderId GetPid(this IHasProviderIds instance, string name)
        {
            if (instance == null)
            {
                return ProviderId.Parse(string.Empty);
            }
            return ProviderId.Parse(instance.GetProviderId(name) ?? string.Empty);
        }

        public static void SetPid(this IHasProviderIds instance, string name, string provider, string id,
            double? position = null, bool? update = null)
        {
            var pid = new ProviderId(provider, id)
            {
                Position = position,
                Update = update
            };
            instance.SetProviderId(name, pid.ToString());
        }

        public static string? GetTrailerUrl(this IHasProviderIds instance)
        {
            if (instance is null)
            {
                return null;
            }

            return !instance.ProviderIds.Any()
                ? string.Empty
                : HttpUtility.UrlDecode(instance.GetProviderId("TrailerUrl"));
        }

        public static void SetTrailerUrl(this IHasProviderIds instance, string url)
        {
            instance.SetProviderId("TrailerUrl", HttpUtility.UrlEncode(url));
        }
    }

}
