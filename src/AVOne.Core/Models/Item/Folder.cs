﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

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
        public virtual bool IsPhysicalRoot => false;
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
