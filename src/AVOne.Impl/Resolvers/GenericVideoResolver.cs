// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Impl.Resolvers
{
    using AVOne.Models.Item;
    using AVOne.Providers;
    using Microsoft.Extensions.Logging;
    /// <summary>
    /// Resolves a Path into an instance of the <see cref="Video"/> class.
    /// </summary>
    /// <typeparam name="T">The type of item to resolve.</typeparam>
    public class GenericVideoResolver<T> : BaseVideoResolver<T>
        where T : Video, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericVideoResolver{T}"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="namingOptions">The naming options.</param>
        public GenericVideoResolver(ILogger logger, IProviderManager providerManager)
            : base(logger, providerManager)
        {
        }
    }
}
