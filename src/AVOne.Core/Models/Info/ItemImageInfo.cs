// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Models.Info
{
    using System.Text.Json.Serialization;
    using AVOne.Enum;

    public class ItemImageInfo
    {
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public ImageType Type { get; set; }

        /// <summary>
        /// Gets or sets the date modified.
        /// </summary>
        /// <value>The date modified.</value>
        public DateTime DateModified { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        [JsonIgnore]
        public bool IsLocalFile => Path == null || !Path.StartsWith("http", StringComparison.OrdinalIgnoreCase);
    }
}
