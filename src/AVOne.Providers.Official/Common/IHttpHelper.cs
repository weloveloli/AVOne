// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Common
{
    public interface IHttpHelper
    {
        public Task<string> GetHtmlAsync(string url, CancellationToken token = default);

        public Task<string> GetHtmlAsync(string url, Dictionary<string, string> headers, CancellationToken token = default);

        /// <summary>
        /// Download the thumb image from the url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<string> DownloadThumbAsync(string url, Dictionary<string, string> headers, CancellationToken token = default);
    }
}
