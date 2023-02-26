// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Abstraction
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
