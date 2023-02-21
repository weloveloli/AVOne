// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Models.Result
{
    using System;
    using System.Collections.Generic;
    using AVOne.Abstraction;

    public class RemoteMetadataSearchResult : IHasProviderIds
    {
        public RemoteMetadataSearchResult()
        {
            ProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the provider ids.
        /// </summary>
        /// <value>The provider ids.</value>
        public Dictionary<string, string> ProviderIds { get; set; }

        public string SearchProviderName { get; set; }

        public string Overview { get; set; }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>The year.</value>
        public int? ProductionYear { get; set; }

        public int? IndexNumber { get; set; }

        public int? IndexNumberEnd { get; set; }

        public int? ParentIndexNumber { get; set; }

        public DateTime? PremiereDate { get; set; }

        public string ImageUrl { get; set; }

        public RemoteMetadataSearchResult AlbumArtist { get; set; }

        public RemoteMetadataSearchResult[] Artists { get; set; }
    }
}
