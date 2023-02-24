// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download
{
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Enum;
    using AVOne.Configuration;
    using AVOne.Constants;
    using AVOne.Enum;
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

        public Task CreateTask(BaseDownloadableItem item, DownloadOpts opts, CancellationToken token = default)
        {
            string url = string.Empty;
            string header = string.Empty;
            string workDir = opts.WorkDir ?? _applicationPaths.CachePath;
            string saveName = opts.PreferName ?? item.Name;
            int threadCount = opts.ThreadCount ?? 1;
            int maxRetry = opts.RetryCount ?? 1;
            long? maxSpeed = opts.MaxSpeed ?? null;
            int interval = 1000;
            bool checkComplete = opts.CheckComplete ?? true;
            int? videoMaxHeight = null;
            string? audioLanguage = null;
            int? noSegStopTime = null;
            var outPutFormat = opts.PreferOutPutFormat ?? OutputFormat.MP4;
            var clearSource = true;
            var clearTempFile = true;
            var quiet = opts.HasListener();
            string? clientName = opts.HttpClientName ?? AVOneConstants.Default;
            VideoDL dl;
            string ffmpegPath = _startupOptions.FFmpegPath ?? "ffmepg";
            if (string.IsNullOrEmpty(clientName))
            {
                dl = new VideoDL(_httpClientFactory.CreateClient(), ffmpegPath);
            }
            else
            {
                dl = new VideoDL(_httpClientFactory.CreateClient(clientName), ffmpegPath);
            }

            if (item is M3U8Item m3u8item)
            {
                if (m3u8item.Url is null)
                {
                    throw Oops.Oh("NOT_A_VALID_DOWNLOADABLE_ITEM");
                }
                url = m3u8item.Url;
                if (m3u8item.Header != null && m3u8item.Header.Any())
                {
                    header = string.Join('|', m3u8item.Header.Select(x => $"{x.Key}:{x.Value}"));
                }
                return dl.DownloadAsync(workDir, saveName, url, header, null, null, threadCount, 200, maxRetry, maxSpeed, interval, checkComplete, videoMaxHeight, audioLanguage, noSegStopTime, token,
                    false, false, false, false, true, false,
                    outPutFormat, clearTempFile, clearSource, quiet, token);
            }
            else if (item is HttpItem httpItem)
            {
                if (httpItem.Url is null)
                {
                    throw Oops.Oh("INVALID_DOWNLOADABLE_ITEM");
                }

                url = httpItem.Url;
                if (httpItem.Header != null && httpItem.Header.Any())
                {
                    header = string.Join('|', httpItem.Header.Select(x => $"{x.Key}:{x.Value}"));
                }
                return dl.DownloadAsync(workDir, saveName, url, header, null, null, threadCount, 200, maxRetry, maxSpeed, interval, checkComplete, videoMaxHeight, audioLanguage, noSegStopTime, token,
                    false, false, false, false, true, false,
                    outPutFormat, clearTempFile, clearSource, quiet, token);
            }

            throw Oops.Oh(ErrorCodes.PROVIDER_NOT_AVAILABLE);
        }

        public bool Support(BaseDownloadableItem item)
        {
            return item is M3U8Item || item is HttpItem;
        }
    }
}
