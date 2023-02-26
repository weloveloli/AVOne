// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Enum
{
    [Flags]
    public enum ItemUpdateType
    {
        None = 1,
        MetadataImport = 2,
        ImageUpdate = 4,
        MetadataDownload = 8,
        MetadataEdit = 16
    }
}
