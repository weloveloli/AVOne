// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Events
{
    using System;
    using AVOne.Providers.Official.Download.Utils;

    public class RecProgressEventArgs
    {
        public TimeSpan RecTime { get; set; }
        public int Finish { get; set; }
        public long DownloadBytes { get; set; }
        public int MaxRetry { get; set; }
        public int Retry { get; set; }
        public long Speed { get; set; }
        public int Lost { get; set; }
        public string Format
        {
            get
            {
                var recTime =
                    RecTime.Hours.ToString("00") + ":" +
                    RecTime.Minutes.ToString("00") + ":" +
                    RecTime.Seconds.ToString("00");
                var downloadSize = Filter.FormatFileSize(DownloadBytes);
                var speed = Filter.FormatFileSize(Speed);
                var print = $@"Progress: {Finish} (REC {recTime}) -- {downloadSize} ({speed}/s) -- Retry ({Retry}/{MaxRetry}) -- Lost {Lost}";
                return print;
            }
        }
    }
}
