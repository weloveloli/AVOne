// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Models.Item
{
    using System.Text.Json.Serialization;
    using AVOne.Abstraction;
    using AVOne.Models.Info;

    public abstract class BaseItem : IHasProviderIds, IHasLookupInfo<ItemLookupInfo>
    {
        /// <summary>
        /// Gets or sets the provider ids.
        /// </summary>
        /// <value>The provider ids.</value>
        [JsonIgnore]
        public Dictionary<string, string> ProviderIds { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [JsonIgnore]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the OriginalTitle.
        /// </summary>
        /// <value>The OriginalTitle.</value>
        [JsonIgnore]
        public string OriginalTitle { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        [JsonIgnore]
        public virtual string Path { get; set; }

        public abstract ItemLookupInfo GetLookupInfo();
    }
}
