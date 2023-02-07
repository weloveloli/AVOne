// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Extensions
{
    using System.Linq;

    public static class ArrayExtensions
    {
        public static bool IsNullOrEmpty<T>(this T[] array) where T : class
        {
            if (array == null || array.Length == 0)
                return true;
            else
                return array.All(item => item == null);
        }
    }
}
