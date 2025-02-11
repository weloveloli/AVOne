// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractor.Base
{
    using System.Threading.Tasks;

    public interface IHttpHelper
    {
        /// <summary>
        /// Get the html content of the url.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<string> GetHtmlAsync(string url, CancellationToken token = default);
    }
}
