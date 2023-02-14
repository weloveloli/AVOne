// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

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
