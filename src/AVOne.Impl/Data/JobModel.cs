// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Data
{
    using System;
    using System.Collections.Generic;
    using LiteDB;

    /// <summary>
    /// Defines the <see cref="JobModel" />.
    /// </summary>
    public class JobModel
    {
        /// <summary>
        /// Gets or sets the id
        /// id......
        /// </summary>
        [BsonId]
        public ObjectId id { get; set; }

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the Modified.
        /// </summary>
        public DateTime Modified { get; set; }

        /// <summary>
        /// Gets or sets the Created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the Key.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets the Extra.
        /// </summary>
        public Dictionary<string, string> Extra { get; set; }
    }
}
