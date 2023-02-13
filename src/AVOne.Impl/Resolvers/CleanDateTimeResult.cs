// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Resolvers
{
    /// <summary>
    /// Holder structure for name and year.
    /// </summary>
    public readonly struct CleanDateTimeResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CleanDateTimeResult"/> struct.
        /// </summary>
        /// <param name="name">Name of video.</param>
        /// <param name="year">Year of release.</param>
        public CleanDateTimeResult(string name, int? year = null)
        {
            Name = name;
            Year = year;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the year.
        /// </summary>
        /// <value>The year.</value>
        public int? Year { get; }
    }
}
