// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Enum
{
    using Furion.FriendlyException;

    [ErrorCodeType]
    public enum ErrorCodes
    {
        [ErrorCodeItemMetadata("PROVIDER_NOT_AVAILABLE")]
        PROVIDER_NOT_AVAILABLE,
        [ErrorCodeItemMetadata("PATH_NOT_EXIST")]
        PATH_NOT_EXIST,
        [ErrorCodeItemMetadata("DIR_NOT_EXIST")]
        DIR_NOT_EXIST,
        [ErrorCodeItemMetadata("SKIP_METADATA_NOT_EXIST")]
        SKIP_METADATA_NOT_EXIST,
        [ErrorCodeItemMetadata("SKIP_FILE_DUE_TO_TARGET_FILE_AREADY_EXIST")]
        SKIP_FILE_DUE_TO_TARGET_FILE_AREADY_EXIST,
        [ErrorCodeItemMetadata("SERVER_ERROR", ErrorCode = "Error")]
        SERVER_ERROR
    }
}
