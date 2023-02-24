// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download
{
    using System;
    using System.Threading.Tasks;
    using AVOne.Common.Enum;
    using AVOne.Configuration;
    using AVOne.Constants;
    using AVOne.Models.Download;
    using AVOne.Providers.Download;
    using Furion.FriendlyException;

    public class DownloaderProvider : IDownloaderProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStartupOptions _startupOptions;
        private readonly IApplicationPaths _applicationPaths;

        public DownloaderProvider(IHttpClientFactory httpClientFactory, IStartupOptions startupOptions, IApplicationPaths applicationPaths)
        {
            _httpClientFactory = httpClientFactory;
            _startupOptions = startupOptions;
            _applicationPaths = applicationPaths;
        }
        public string Name => "Official";

        public int Order => 1;

        public Task CreateTask(BaseDownloadableItem item, DownloadOpts opts)
        {
            if (item is M3U8Item m3u8item)
            {
                var clientName = opts.HttpClientName ?? AVOneConstants.Default;
                var dl = new VideoDL(_httpClientFactory.CreateClient(clientName), _startupOptions.FFmpegPath);
                
            }

            throw Oops.Oh(ErrorCodes.PROVIDER_NOT_AVAILABLE);
        }

        public bool Support(BaseDownloadableItem item)
        {
            return item is M3U8Item;
        }
    }
}
