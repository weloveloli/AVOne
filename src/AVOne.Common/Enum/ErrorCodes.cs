// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Enum
{
    using Furion.FriendlyException;

    [ErrorCodeType]
    public enum ErrorCodes
    {
        [ErrorCodeItemMetadata("ProviderNotAvailable")]
        ProviderNotAvailable,

        [ErrorCodeItemMetadata("服务器运行异常", ErrorCode = "Error")]
        SERVER_ERROR
    }
}
