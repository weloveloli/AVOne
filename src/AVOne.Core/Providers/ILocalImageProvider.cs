// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Providers
{
    using AVOne.IO;
    using AVOne.Models.Info;
    using AVOne.Models.Item;

    /// <summary>
    /// This is just a marker interface.
    /// </summary>
    public interface ILocalImageProvider : IImageProvider
    {
        IEnumerable<LocalImageInfo> GetImages(BaseItem item, IDirectoryService directoryService);
    }
}
