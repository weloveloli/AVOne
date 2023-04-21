// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Metadata
{
#nullable disable

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
