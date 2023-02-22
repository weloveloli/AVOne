// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Metadata
{
    using AVOne.Models.Item;

    public interface IMetadataFileSaverProvider : IMetadataSaverProvider
    {
        /// <summary>
        /// Gets the save path.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>System.String.</returns>
        string GetSavePath(BaseItem item);
    }
}
