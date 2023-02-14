// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Enum
{
    /// <summary>
    /// Enum ResolverPriority.
    /// </summary>
    public enum ResolverPriority
    {
        /// <summary>
        /// The highest priority. Used by plugins to bypass the default server resolvers.
        /// </summary>
        Plugin = 0,

        /// <summary>
        /// The first.
        /// </summary>
        First = 1,

        /// <summary>
        /// The second.
        /// </summary>
        Second = 2,

        /// <summary>
        /// The third.
        /// </summary>
        Third = 3,

        /// <summary>
        /// The Fourth.
        /// </summary>
        Fourth = 4,

        /// <summary>
        /// The Fifth.
        /// </summary>
        Fifth = 5,

        /// <summary>
        /// The last.
        /// </summary>
        Last = 6
    }
}
