// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable


namespace AVOne.Providers.Metadata
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
