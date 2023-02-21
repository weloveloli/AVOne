// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Configuration
{
    using System.Collections.Generic;

    public class AVOneConfiguration
    {
        public List<string> ScanMetaDataProviders { get; set; } = new List<string> { "MetaTube", "Jellyfin.Nfo" };
        public List<string> ImageMetaDataProviders { get; set; } = new List<string> { "MetaTube" };
    }
}
