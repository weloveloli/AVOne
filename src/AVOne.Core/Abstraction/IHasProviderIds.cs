// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Core.Abstraction
{
    using System.Collections.Generic;

    public interface IHasProviderIds
    {
        /// <summary>
        /// Gets or sets the provider ids.
        /// </summary>
        /// <value>The provider ids.</value>
        Dictionary<string, string> ProviderIds { get; set; }
    }
}
