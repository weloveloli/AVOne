// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime? GetValidDateTime(this DateTime dateTime)
        {
            return dateTime.Year > 1 ? dateTime : null;
        }

        public static int? GetValidYear(this DateTime dateTime)
        {
            return dateTime.GetValidDateTime()?.Year;
        }
    }
}
