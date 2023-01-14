// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Core.Entity
{
    using System.Text.Json.Serialization;
    using AVOne.Core.Abstraction;

    public abstract class BaseItem : IHasProviderIds, IHasLookupInfo<ItemLookupInfo>, IEquatable<BaseItem>
    {
        /// <summary>
        /// Gets or sets the provider ids.
        /// </summary>
        /// <value>The provider ids.</value>
        [JsonIgnore]
        public Dictionary<string, string>? ProviderIds { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [JsonIgnore]
        public virtual string? Name { get; set; }

        /// <summary>
        /// Gets or sets the OriginalTitle.
        /// </summary>
        /// <value>The OriginalTitle.</value>
        [JsonIgnore]
        public string? OriginalTitle { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        [JsonIgnore]
        public virtual string? Path { get; set; }
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [JsonIgnore]
        public Guid Id { get; set; }

        ItemLookupInfo IHasLookupInfo<ItemLookupInfo>.GetLookupInfo()
        {
            return GetItemLookupInfo<ItemLookupInfo>();
        }

        /// <inheritdoc />
        bool IEquatable<BaseItem>.Equals(BaseItem other)
        {
            return other is not null && other.Id.Equals(Id);
        }

        protected T GetItemLookupInfo<T>()
            where T : ItemLookupInfo, new()
        {
            return new T
            {
                Path = Path,
                Name = GetNameForMetadataLookup(),
                OriginalTitle = OriginalTitle,
            };
        }

        protected virtual string GetNameForMetadataLookup()
        {
            return Name;
        }
    }
}
