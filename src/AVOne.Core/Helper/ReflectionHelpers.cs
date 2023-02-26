// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Helper
{
    using System;
    using System.ComponentModel;

    public static class ReflectionHelpers
    {
        public static string GetCustomDescription(object? objEnum)
        {
            if (objEnum == null)
            {
                return string.Empty;
            }

            var fi = objEnum.GetType().GetField(objEnum!.ToString()!)!;
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes != null && attributes!.Length > 0) ? attributes[0]!.Description : objEnum.ToString()!;
        }

        public static string Description(this Enum value)
        {
            return GetCustomDescription(value);
        }
    }
}
