// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace MediaBrowser.Controller.Library
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
