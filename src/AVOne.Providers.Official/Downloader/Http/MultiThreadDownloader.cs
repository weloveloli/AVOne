// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.Http
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Configuration;
    using AVOne.Extensions;
    using AVOne.Models.Download;
    using Microsoft.Extensions.Logging;

    internal class MultiThreadDownloader
    {
        private IConfigurationManager _configurationManager;
        private HttpClient _client;
        private ILogger<HttpDownloadProvider> _logger;
        private IApplicationPaths _applicationPaths;

        public MultiThreadDownloader(IConfigurationManager configurationManager, HttpClient client, ILogger<HttpDownloadProvider> logger, IApplicationPaths applicationPaths)
        {
            _configurationManager = configurationManager;
            _client = client;
            _logger = logger;
            _applicationPaths = applicationPaths;
        }

        internal Task CreateTask(BaseDownloadableItem item, DownloadOpts opts, string outputFile, CancellationToken token)
        {
            var httpItem = item as HttpItem;
            var threadCount = opts.ThreadCount ?? _configurationManager.CommonConfiguration.DownloadConfig.DefaultDownloadThreadCount;
            var workDir = opts.WorkDir;
            var downloadUrl = httpItem.Url;
            if (workDir == null)
            {
                workDir = Path.Combine(_applicationPaths.CachePath, downloadUrl.GetMD5().ToString());
            }

            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir);
            }

            var tmpFile = Path.Combine(workDir, Path.GetFileName(outputFile));

            var fileLength = httpItem.Size;

            var blockSize = fileLength / threadCount;
            var tasks = new List<Task>();
            for (var i = 0; i < threadCount; i++)
            {
                var start = i * blockSize;
                var end = (i + 1) * blockSize - 1;
                if (i == threadCount - 1)
                {
                    end = fileLength - 1;
                }
                var task = DownloadBlockAsync(downloadUrl, tmpFile, start, end, token);
                tasks.Add(task);
            }

            return Task.WhenAll(tasks).ContinueWith((t) =>
            {
                if (t.IsCompleted)
                {
                    File.Move(tmpFile, outputFile);
                }
            }, token);
        }

        private Task DownloadBlockAsync(string downloadUrl, string tmpFile, long? start, long? end, CancellationToken token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            if (start != null && end != null)
            {
                request.Headers.Range = new RangeHeaderValue(start.Value, end.Value);
            }
            // use ResponseHeadersRead to avoid read stream to memory
            return _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token)
                .ContinueWith(async (t) =>
                {
                    var response = t.Result;
                    if (response.IsSuccessStatusCode)
                    {
                        using (var fs = new FileStream(tmpFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                        {
                            fs.Seek(start.Value, SeekOrigin.Begin);
                            // use buffer to improve performance

                            var buffer = new byte[1024 * 1024];
                            var stream = await response.Content.ReadAsStreamAsync();
                            var read = 0;
                            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                            {
                                await fs.WriteAsync(buffer, 0, read, token);
                            }

                            await fs.FlushAsync(token);
                        }
                    }
                }, token);
        }
    }
}
