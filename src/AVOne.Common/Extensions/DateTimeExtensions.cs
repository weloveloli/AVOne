// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Extensions
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
