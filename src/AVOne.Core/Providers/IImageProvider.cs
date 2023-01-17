// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Providers
{
    using AVOne.Abstraction;
    using AVOne.Models.Item;

    /// <summary>
    /// Interface IImageProvider.
    /// </summary>
    public interface IImageProvider : IProvider
    {
        /// <summary>
        /// Supports the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the provider supports the item.</returns>
        bool Supports(BaseItem item);
    }
}
