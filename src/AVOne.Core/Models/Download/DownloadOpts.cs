// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Models.Download
{
    using AVOne.Constants;
    using AVOne.Enum;
    using AVOne.Models.Job;

    public class DownloadOpts
    {
        public string? HttpClientName { get; set; } = HttpClientNames.Default;
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
        public bool Overwrite { get; set; } = false;
        public bool CheckComplete { get; set; } = false;

        /// <summary>
        /// Occurs when [item added].
        /// </summary>
        public event EventHandler<JobStatusArgs>? StatusChanged;

        public void OnStatusChanged(JobStatusArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        public bool HasListener()
        {
            return StatusChanged != null;
        }
    }
}
