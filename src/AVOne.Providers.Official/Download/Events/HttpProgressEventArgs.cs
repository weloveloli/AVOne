// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.Events
{
    using AVOne.Providers.Official.Download.Utils;

    public class HttpProgressEventArgs
    {
        public double? Percentage { get; set; }
        public long? TotalBytes { get; set; }
        public long DownloadBytes { get; set; }
        public int MaxRetry { get; set; }
        public int Retry { get; set; }
        public long Speed { get; set; }
        public int? Eta { get; set; }
        public string Format
        {
            get
            {
                var downloadSize = Filter.FormatFileSize(DownloadBytes);
                var speed = Filter.FormatFileSize(Speed);
                var percentage = Percentage != null ?
                    $"({Percentage.Value * 100:0.00} %) -- " : "";
                var totalSize = TotalBytes != null ?
                    $"/{Filter.FormatFileSize(TotalBytes.Value)}" : "";
                var eta = Eta != null ?
                    $" @ {Filter.FormatTime(Eta.Value)}" : "";
                var print = $@"Progress: {percentage}{downloadSize}{totalSize} ({speed}/s{eta}) -- Retry ({Retry}/{MaxRetry})";
                return print;
            }
        }
    }
}
