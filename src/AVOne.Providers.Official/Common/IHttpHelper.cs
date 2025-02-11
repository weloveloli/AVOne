// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Common
{
    public interface IHttpHelper
    {
        public Task<string> GetHtmlAsync(string url, CancellationToken token = default);
    }
}
