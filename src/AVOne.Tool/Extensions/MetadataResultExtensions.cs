// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Extensions
{
    using AVOne.Configuration;
    using AVOne.Models.Result;
    using AVOne.Providers;

    public static class MetadataResultExtensions
    {
        public static List<NameValue> NameValues<T>(this MetadataResult<T> metadataResult, IProvider provider)
        {
            // return null when metadataResult is null or HasMetadata is false
            if( metadataResult == null || metadataResult.HasMetadata == null)
            {
                return null;
            }
        }
    }
}
