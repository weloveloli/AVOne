// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

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
        public JobModel()
        {
            this.Tags = new List<string>();
        }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; }

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
        /// Gets or sets the Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Progress.
        /// </summary>
        /// <value>
        /// The Progress value.
        /// </value>
        public double Progress { get; set; }

        public List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        public string JobStatus { get; set; }

        public string Error { get; set; }
        /// <summary>
        /// Gets or sets the Extra.
        /// </summary>
        public Dictionary<string, string> Extra { get; set; }
    }
}
