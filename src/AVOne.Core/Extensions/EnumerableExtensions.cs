// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Static extensions for the <see cref="IEnumerable{T}"/> interface.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determines whether the value is contained in the source collection.
        /// </summary>
        /// <param name="source">An instance of the <see cref="IEnumerable{String}"/> interface.</param>
        /// <param name="value">The value to look for in the collection.</param>
        /// <param name="stringComparison">The string comparison.</param>
        /// <returns>A value indicating whether the value is contained in the collection.</returns>
        /// <exception cref="ArgumentNullException">The source is null.</exception>
        public static bool Contains(this IEnumerable<string> source, ReadOnlySpan<char> value, StringComparison stringComparison)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source is IList<string> list)
            {
                var len = list.Count;
                for (var i = 0; i < len; i++)
                {
                    if (value.Equals(list[i], stringComparison))
                    {
                        return true;
                    }
                }

                return false;
            }

            foreach (var element in source)
            {
                if (value.Equals(element, stringComparison))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
