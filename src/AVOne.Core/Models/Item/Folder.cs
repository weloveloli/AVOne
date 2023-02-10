// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Models.Item
{
    using System.Text.Json.Serialization;
    using AVOne.Models.Info;

    public class Folder : BaseItem
    {
        public Folder()
        {
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance is root.
        /// </summary>
        /// <value><c>true</c> if this instance is root; otherwise, <c>false</c>.</value>
        public bool IsRoot { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is folder.
        /// </summary>
        /// <value><c>true</c> if this instance is folder; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public override bool IsFolder => true;

        public override ItemLookupInfo GetLookupInfo()
        {
            throw new NotImplementedException();
        }
    }
}
