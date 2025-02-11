// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Models.Download
{
    using AVOne.Models.Job;

    public class DownloadFinishEventArgs : JobStatusArgs
    {
        public DownloadFinishEventArgs()
        {
            Status = "Finish";
            Progress = 100;
        }
        public string FinalFilePath { get; set; }

        public long TotalFileBytes { get; set; }
    }
}
