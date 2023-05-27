// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.Http
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Helper;
    using AVOne.Configuration;
    using AVOne.Constants;
    using AVOne.Enum;
    using AVOne.Models.Download;
    using AVOne.Providers.Download;
    using Microsoft.Extensions.Logging;

    // Implement IDownloaderProvider to provide a downloader for a specific website.
    public class HttpDownloadProvider : HttpClientHelper, IDownloaderProvider
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly HttpClient _client;
        private readonly IConfigurationManager _configurationManager;

        private readonly ILogger<HttpDownloadProvider> logger;
        public HttpDownloadProvider(IConfigurationManager manager, IHttpClientFactory httpClientFactory, ILogger<HttpDownloadProvider> logger, IApplicationPaths applicationPaths) : base(manager, httpClientFactory)
        {
            this.logger = logger;
            this._configurationManager = manager;
            this._client = GetHttpClient(HttpClientNames.Download);
            _applicationPaths = applicationPaths;
        }

        public string DisplayName => "Default Http Downloader";

        public string Name => "DefaultHttp";

        public int Order => (int)ProviderOrder.Default;

        /// <summary>
        /// Use multithread to download the file.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="opts">The opts.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task CreateTask(BaseDownloadableItem item, DownloadOpts opts, CancellationToken token = default)
        {
            if (item is not HttpItem httpItem)
            {
                throw new ArgumentException("The item is not a HttpItem.");
            }
            if (opts == null)
            {
                throw new ArgumentNullException(nameof(opts));
            }
            if (opts.OutputDir == null)
            {
                throw new ArgumentNullException(nameof(opts.OutputDir));
            }

            var downloadUrl = httpItem.Url;
            /// check if the downloadUrl is valid.
            if (string.IsNullOrEmpty(downloadUrl))
            {
                logger.LogWarning($"The download url is empty.");
                return Task.CompletedTask;
            }

            /// check if the file already exists.
            var fileName = opts.PreferName ?? item.SaveName!;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = downloadUrl.Split('/').Last();
            }

            /// Use multithread to download the file.
            /// Check if the downloadUrl support multithread.
            httpItem.HttpRangeSupport = CheckDownloadLink(httpItem, downloadUrl, token);
            var outputFilePath = Path.Combine(opts.OutputDir, fileName + httpItem.Extension);
            if (File.Exists(outputFilePath))
            {
                if (!opts.Overwrite)
                {
                    logger.LogWarning($"The file {outputFilePath} already exists.");
                    //outputFilePath = Path.Combine(opts.OutputDir, $"{fileName}_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}{httpItem.Extension}");
                    opts.OnStatusChanged(new DownloadFinishEventArgs { Status = "Download finished", FinalFilePath = outputFilePath, Progress = 100 });
                    return Task.CompletedTask;
                }
                else
                {
                    File.Delete(outputFilePath);
                }
            }

            if (httpItem.HttpRangeSupport == HttpRangeSupport.Yes)
            {
                /// call the multithread downloader.
                var downloader = new MultiThreadDownloader(_configurationManager, _client, _applicationPaths);
                return downloader.CreateTask(httpItem, opts, outputFilePath, token);
            }
            else
            {
                opts.ThreadCount = 1;
                /// call the single thread downloader.
                var downloader = new MultiThreadDownloader(_configurationManager, _client, _applicationPaths);
                return downloader.CreateTask(httpItem, opts, outputFilePath, token);
            }
        }

        private HttpRangeSupport CheckDownloadLink(HttpItem httpItem, string downloadUrl, CancellationToken token)
        {
            var httpRangeSupport = httpItem.HttpRangeSupport;
            if (httpRangeSupport == HttpRangeSupport.Unknown || string.IsNullOrEmpty(httpItem.Extension) || !httpItem.Size.HasValue)
            {
                var req = new HttpRequestMessage(HttpMethod.Head, downloadUrl);
                var resp = _client.SendAsync(req, token).Result;
                resp.EnsureSuccessStatusCode();

                var acceptRanges = resp.Headers.AcceptRanges;
                var mimeType = resp.Content.Headers.ContentType?.MediaType;
                if (string.IsNullOrEmpty(mimeType))
                {
                    throw new ArgumentException("Can not detect mime type of download link");
                }
                var extension = MimeTypesHelper.ToExtension(mimeType);
                httpItem.Extension = extension;
                httpItem.Size = resp.Content.Headers.ContentLength;
                if (acceptRanges != null && acceptRanges.Contains("bytes") && httpItem.Size.HasValue)
                {
                    httpRangeSupport = HttpRangeSupport.Yes;
                }
                else
                {
                    httpRangeSupport = HttpRangeSupport.No;
                }
            }

            return httpRangeSupport;
        }

        public bool Support(BaseDownloadableItem item)
        {
            return item is not null && item is HttpItem;
        }
    }
}
