// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Core.Models
{
    public class MetadataResult<T>
    {
        public bool HasMetadata { get; set; }

        public T? Item { get; set; }

        public string? Provider { get; set; }

        public bool QueriedById { get; set; }
    }
}
