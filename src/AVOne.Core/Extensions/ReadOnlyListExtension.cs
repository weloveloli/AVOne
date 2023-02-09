// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Static extensions for the <see cref="IReadOnlyList{T}"/> interface.
    /// </summary>
    public static class ReadOnlyListExtension
    {
        /// <summary>
        /// Finds the index of the desired item.
        /// </summary>
        /// <param name="source">The source list.</param>
        /// <param name="value">The value to fine.</param>
        /// <typeparam name="T">The type of item to find.</typeparam>
        /// <returns>Index if found, else -1.</returns>
        public static int IndexOf<T>(this IReadOnlyList<T> source, T value)
        {
            if (source is IList<T> list)
            {
                return list.IndexOf(value);
            }

            for (var i = 0; i < source.Count; i++)
            {
                if (Equals(value, source[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Finds the index of the predicate.
        /// </summary>
        /// <param name="source">The source list.</param>
        /// <param name="match">The value to find.</param>
        /// <typeparam name="T">The type of item to find.</typeparam>
        /// <returns>Index if found, else -1.</returns>
        public static int FindIndex<T>(this IReadOnlyList<T> source, Predicate<T> match)
        {
            if (source is List<T> list)
            {
                return list.FindIndex(match);
            }

            for (var i = 0; i < source.Count; i++)
            {
                if (match(source[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get the first or default item from a list.
        /// </summary>
        /// <param name="source">The source list.</param>
        /// <typeparam name="T">The type of item.</typeparam>
        /// <returns>The first item or default if list is empty.</returns>
        public static T? FirstOrDefault<T>(this IReadOnlyList<T>? source)
        {
            return source is null || source.Count == 0 ? default : source[0];
        }

        /// <summary>
        /// Copies all the elements of the current collection to the specified list
        /// starting at the specified destination array index. The index is specified as a 32-bit integer.
        /// </summary>
        /// <param name="source">The current collection that is the source of the elements.</param>
        /// <param name="destination">The list that is the destination of the elements copied from the current collection.</param>
        /// <param name="index">A 32-bit integer that represents the index in <c>destination</c> at which copying begins.</param>
        /// <typeparam name="T">The type of the array.</typeparam>
        public static void CopyTo<T>(this IReadOnlyList<T> source, IList<T> destination, int index = 0)
        {
            for (int i = 0; i < source.Count; i++)
            {
                destination[index + i] = source[i];
            }
        }
    }
}
