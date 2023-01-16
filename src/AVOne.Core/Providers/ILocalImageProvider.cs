// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers
{
    using AVOne.Entities;
    using AVOne.Models.Info;

    /// <summary>
    /// This is just a marker interface.
    /// </summary>
    public interface ILocalImageProvider : IImageProvider
    {
        IEnumerable<LocalImageInfo> GetImages(BaseItem item, IDirectoryService directoryService);
    }
}
