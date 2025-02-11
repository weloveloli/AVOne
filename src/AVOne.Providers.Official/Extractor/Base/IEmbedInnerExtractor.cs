// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Base
{
    using AVOne.Models.Download;

    public interface IEmbedInnerExtractor
    {
        /// <summary>
        /// Whether the embed url is supported, used to judge if the emebed provider should be choosen or not
        /// </summary>
        /// <returns></returns>
        public bool IsEmbedUrlSupported(string embedUrl);

        /// <summary>
        /// Get the download items from the embed url.
        /// </summary>
        /// <param name="parnentWebPageUrl"></param>
        /// <param name="parentHtmlContent"></param>
        /// <param name="embedWebPageUrl"></param>
        /// <param name="embedHtmlContent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<IEnumerable<BaseDownloadableItem>> ExtractFromEmbedPageAsync(
            string parnentWebPageUrl,
            string parentHtmlContent,
            string embedWebPageUrl,
            string embedHtmlContent,
            CancellationToken token = default);
    }
}
