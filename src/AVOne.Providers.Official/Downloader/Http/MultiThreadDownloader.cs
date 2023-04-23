// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.Http
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Configuration;
    using AVOne.Extensions;
    using AVOne.Models.Download;

    internal class MultiThreadDownloader
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly HttpClient _client;
        private readonly IApplicationPaths _applicationPaths;

        public MultiThreadDownloader(IConfigurationManager configurationManager, HttpClient client, IApplicationPaths applicationPaths)
        {
            _configurationManager = configurationManager;
            _client = client;
            _applicationPaths = applicationPaths;
        }

        internal async Task CreateTask(HttpItem item, DownloadOpts opts, string outputFile, CancellationToken token)
        {
            var threadCount = opts.ThreadCount ?? _configurationManager.CommonConfiguration.DownloadConfig.DefaultDownloadThreadCount;
            var workDir = opts.WorkDir;
            var downloadUrl = item.Url;
            if (workDir == null)
            {
                workDir = Path.Combine(_applicationPaths.CachePath, downloadUrl.GetMD5().ToString());
            }
            else
            {
                workDir = Path.Combine(_applicationPaths.CachePath, downloadUrl.GetMD5().ToString());
            }

            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir);
            }

            var tmpFile = Path.Combine(workDir, Path.GetFileName(outputFile));

            var fileLength = item.Size!;

            var blockSize = fileLength / threadCount;
            var tasks = new List<Task>();
            int interval = 1000;
            var totalBytes = fileLength ?? 0L;
            var downloadBytes = 0L;
            var intervalDownloadBytes = 0L;
            var stop = false;
            var timer = new System.Timers.Timer(interval)
            {
                AutoReset = true
            };
            timer.Elapsed += delegate
            {
                if (!stop)
                {
                    ProgressEvent(opts, interval, downloadBytes, intervalDownloadBytes, totalBytes);
                    Interlocked.Exchange(ref intervalDownloadBytes, 0);
                }
            };
            timer.Enabled = true;
            for (var i = 0; i < threadCount; i++)
            {
                var start = i * blockSize;
                var end = (i + 1) * blockSize - 1;
                if (i == threadCount - 1)
                {
                    end = fileLength - 1;
                }
                var task = DownloadBlockAsync(downloadUrl, tmpFile, start, end, (d) =>
                {
                    Interlocked.Add(ref intervalDownloadBytes, d);
                    Interlocked.Add(ref downloadBytes, d);
                }, token);
                tasks.Add(task);
            }
            timer.Start();
            try
            {
                await Task.WhenAll(tasks);
                File.Move(tmpFile, outputFile);
                var fileFileInfo = new FileInfo(outputFile);
                Directory.Delete(workDir);
                opts.OnStatusChanged(new DownloadFinishEventArgs { FinalFilePath = outputFile, Progress = 100, TotalFileBytes = fileFileInfo.Length });
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                timer.Stop();
                timer.Enabled = false;
                stop = true;
            }
        }

        private async Task DownloadBlockAsync(string downloadUrl, string tmpFile, long? start, long? end, Action<long> downloadBytesAction, CancellationToken token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            if (start != null && end != null)
            {
                request.Headers.Range = new RangeHeaderValue(start.Value, end.Value);
            }
            // use ResponseHeadersRead to avoid read stream to memory
            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();
            using var fs = new FileStream(tmpFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            fs.Seek(start.Value, SeekOrigin.Begin);
            // use buffer to improve performance
            var buffer = new byte[64 * 1024];
            var stream = await response.Content.ReadAsStreamAsync();
            var read = 0;
            while ((read = await stream.ReadAsync(buffer, token)) > 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, read), token);
                downloadBytesAction(read);
            }

            await fs.FlushAsync(token);
        }

        private static void ProgressEvent(DownloadOpts opts, int interval, long downloadBytes, long intervalDownloadBytes, long totalBytes)
        {
            try
            {
                var percentage = Div(downloadBytes, totalBytes);
                var speed = intervalDownloadBytes / interval * 1000;
                var eta = (int)Div(totalBytes - downloadBytes, speed);
                var args = new DownloadProgressEventArgs
                {
                    DownloadBytes = downloadBytes,
                    MaxRetry = opts.RetryCount ?? 1,
                    Retry = 0,
                    Precentage = percentage,
                    TotalBytes = totalBytes,
                    Speed = speed,
                    Eta = eta
                };
                opts.OnStatusChanged(args);

            }
            catch { }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Div(double a, double b)
        {
            return b == 0 ? 0 : a / b;
        }
    }
}
