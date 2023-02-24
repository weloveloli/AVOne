// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Models.Download
{
    using AVOne.Enum;

    public class DownloadOpts
    {
        public string? HttpClientName { get; set; }
        public string? PreferName { get; set; }
        public OutputFormat? PreferOutPutFormat { get; set; }
        public MuxOutputFormat? PreferMuxOutPutFormat { get; set; }
        public string? OutputDir { get; set; }
        public string? WorkDir { get; set; }
        public int? ThreadCount { get; set; }
        public int? RetryCount { get; set; }
        public int? RetryWait { get; set; }
        public long? MaxSpeed { get; set; }
        public int? Timeout { get; set; }
        public bool? Overwrite { get; set; }
        public bool? CheckComplete { get; set; }

        /// <summary>
        /// Occurs when [item added].
        /// </summary>
        public event EventHandler<DownloadStatusArgs>? StatusChanged;

        public void OnStatusChanged(DownloadStatusArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        public bool HasListener()
        {
            return StatusChanged != null;
        }
    }
}
