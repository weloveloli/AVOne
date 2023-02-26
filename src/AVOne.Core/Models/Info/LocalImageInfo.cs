// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Models.Info
{
    using AVOne.Enum;
    using AVOne.IO;

    public class LocalImageInfo
    {
        public FileSystemMetadata FileInfo { get; set; }

        public ImageType Type { get; set; }
    }
}
