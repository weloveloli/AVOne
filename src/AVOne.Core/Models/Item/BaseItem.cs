// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Models.Item
{
    using System.Text.Json.Serialization;
    using AVOne.Abstraction;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Helper;
    using AVOne.IO;
    using AVOne.Models.Info;
    using Microsoft.Extensions.Logging;

    public abstract class BaseItem : MetaDataItem, IHasProviderIds, IHasLookupInfo<ItemLookupInfo>
    {
        protected BaseItem()
            : base()
        {
            ProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            ProductionLocations = Array.Empty<string>();
            RemoteTrailers = Array.Empty<MediaUrl>();
        }

        /// <summary>
        /// The supported image extensions.
        /// </summary>
        public static readonly string[] SupportedImageExtensions
            = new[] { ".png", ".jpg", ".jpeg", ".tbn", ".gif" };

        public static IFileSystem FileSystem { get; set; }
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public static ILogger<BaseItem> Logger { get; set; }

        public static IConfigurationManager ConfigurationManager { get; set; }

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

        public DateTime DateLastSaved { get; set; }

        /// <summary>
        /// Gets or sets the date created.
        /// </summary>
        /// <value>The date created.</value>
        [JsonIgnore]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the date modified.
        /// </summary>
        /// <value>The date modified.</value>
        [JsonIgnore]
        public DateTime DateModified { get; set; }

        [JsonIgnore]
        public string Tagline { get; set; }

        public abstract ItemLookupInfo GetLookupInfo();

        /// <summary>
        /// Gets or sets the name of the sort.
        /// </summary>
        /// <value>The name of the sort.</value>
        [JsonIgnore]
        public string SortName { get; set; }

        public string ForcedSortName { get; set; }

        [JsonIgnore]
        public string PreferredMetadataLanguage { get; set; }
        public string PreferredMetadataCountryCode { get; set; }
        public string[] ProductionLocations { get; set; }

        /// <summary>
        /// Gets or sets the remote trailers.
        /// </summary>
        /// <value>The remote trailers.</value>
        public IReadOnlyList<MediaUrl> RemoteTrailers { get; set; }

        /// <summary>
        /// Gets or sets the date that the item first debuted. For movies this could be premiere date, episodes would be first aired.
        /// </summary>
        /// <value>The premiere date.</value>
        [JsonIgnore]
        public DateTime? PremiereDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>The end date.</value>
        [JsonIgnore]
        public DateTime? EndDate { get; set; }

        public bool SupportsLocalMetadata { get; set; } = true;
        /// <summary>
        /// Gets or sets the run time ticks.
        /// </summary>
        /// <value>The run time ticks.</value>
        [JsonIgnore]
        public long? RunTimeTicks { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is folder.
        /// </summary>
        /// <value><c>true</c> if this instance is folder; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public virtual bool IsFolder => false;

        [JsonIgnore]
        public ExtraType? ExtraType { get; set; }

        /// <summary>
        /// Gets the folder containing the item.
        /// If the item is a folder, it returns the folder itself.
        /// </summary>
        [JsonIgnore]
        public virtual string ContainingFolderPath
        {
            get
            {
                if (IsFolder)
                {
                    return Path;
                }

                return System.IO.Path.GetDirectoryName(Path);
            }
        }
        [JsonIgnore]
        public virtual bool IsTopParent => false;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is in mixed folder.
        /// </summary>
        /// <value><c>true</c> if this instance is in mixed folder; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsInMixedFolder { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [JsonIgnore]
        public Guid Id { get; set; }

        [JsonIgnore]
        public Guid OwnerId { get; set; }

        [JsonIgnore]
        public Guid ParentId { get; set; }

        [JsonIgnore]
        public bool IsFileProtocol => true;

        [JsonIgnore]
        public virtual string FileNameWithoutExtension
        {
            get
            {
                if (IsFileProtocol)
                {
                    return System.IO.Path.GetFileNameWithoutExtension(Path);
                }

                return null;
            }
        }

        public void SetParent(Folder parent)
        {
            ParentId = parent == null ? Guid.Empty : parent.Id;
        }

        private string _targetName;

        public string TargetName
        {
            get
            {
                return _targetName ?? Name;
            }

            set
            {
                _targetName = value;
            }
        }

        private string _targetPath;

        public string TargetPath
        {
            get
            {
                return _targetPath ?? Path;
            }

            set
            {
                _targetPath = value;
            }
        }
    }
}
