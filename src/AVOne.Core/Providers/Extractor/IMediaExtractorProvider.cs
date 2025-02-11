// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Extractor
{
    using AVOne.Models.Download;

    public interface IMediaExtractorProvider : IOrderProvider
    {
        public bool Support(string webPageUrl);

        public Task<IEnumerable<BaseDownloadableItem>> ExtractAsync(string webPageUrl, CancellationToken token = default);
    }
}
