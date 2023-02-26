// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Extensions
{
    using System.Linq;

    public static class ArrayExtensions
    {
        public static bool IsNullOrEmpty<T>(this T[] array) where T : class
        {
            return array == null || array.Length == 0 || array.All(item => item == null);
        }
    }
}
