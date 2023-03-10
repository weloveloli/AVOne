// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.M3U8.Utils
{
    using AVOne.Models.Download;

    public class DownloadProgressEventArgs : DownloadStatusArgs
    {
        public int Total { get; set; }
        public int Finish { get; set; }
        public double Percentage { get; set; }
        public long TotalBytes { get; set; }
        public long DownloadBytes { get; set; }
        public int MaxRetry { get; set; }
        public int Retry { get; set; }
        public long Speed { get; set; }
        public int Eta { get; set; }
        public override string Status
        {
            get
            {
                var percentage = (Percentage * 100).ToString("0.00");
                var totalSize = FormatFileSize(TotalBytes);
                var downloadSize = FormatFileSize(DownloadBytes);
                var speed = FormatFileSize(Speed);
                var eta = FormatTime(Eta);
                var print = $"{Finish}/{Total} ({percentage} %) -- {downloadSize}/{totalSize} ({speed}/s @ {eta}) -- Retry ({Retry}/{MaxRetry})";
                return print;
            }
        }

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
                return string.Format("{0:########0.00} GB", ((double)fileSize) / (1024 * 1024 * 1024));
            }
            else if (fileSize >= 1024 * 1024)
            {
                return string.Format("{0:####0.00} MB", ((double)fileSize) / (1024 * 1024));
            }
            else if (fileSize >= 1024)
            {
                return string.Format("{0:####0.00} KB", ((double)fileSize) / 1024);
            }
            else
            {
                return string.Format("{0} bytes", fileSize);
            }
        }
    }
}
