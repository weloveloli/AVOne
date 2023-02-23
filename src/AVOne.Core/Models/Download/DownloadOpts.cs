// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Models.Download
{
    public class DownloadOpts
    {
        public string? OutputPath { get; set; }
        public string? WorkDir { get; set; }
        public int? ThreadCount { get; set; }
        public int? RetryCount { get; set; }
        public int? RetryWait { get; set; }
        public int? Timeout { get; set; }
        public bool? Overwrite { get; set; }
        /// <summary>
        /// Occurs when [item added].
        /// </summary>
        public event EventHandler<DownloadStatusArgs>? StatusChanged;

        public void OnStatusChanged(DownloadStatusArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }
    }
}
