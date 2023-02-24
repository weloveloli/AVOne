// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Utils
{
    using System;

    public class Filter
    {
        public static string FormatTime(int time)
        {
            var ts = new TimeSpan(0, 0, time);
            var str = (ts.Hours.ToString("00") == "00" ? "" : ts.Hours.ToString("00") + "h") + ts.Minutes.ToString("00") + "m" + ts.Seconds.ToString("00") + "s";
            return str;
        }

        public static string FormatFileSize(double fileSize)
        {
            if (fileSize < 0)
            {
                return "Error";
            }
            else if (fileSize >= 1024 * 1024 * 1024)
            {
                return string.Format("{0:########0.00} GB", (double)fileSize / (1024 * 1024 * 1024));
            }
            else
            {
                return fileSize >= 1024 * 1024
                    ? string.Format("{0:####0.00} MB", (double)fileSize / (1024 * 1024))
                    : fileSize >= 1024 ? string.Format("{0:####0.00} KB", (double)fileSize / 1024) : string.Format("{0} bytes", fileSize);
            }
        }
    }
}
