// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Download
{
    using AVOne.Models.Download;

    public interface IDownloaderProvider : IOrderProvider
    {
        public bool Support(BaseDownloadableItem item);

        public Task CreateTask(BaseDownloadableItem item, DownloadOpts opts);
    }
}
