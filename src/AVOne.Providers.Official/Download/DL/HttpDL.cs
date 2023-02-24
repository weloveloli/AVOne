// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.DL
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Providers.Official.Download.Events;
    using AVOne.Providers.Official.Download.Utils;

    public class HttpDL : BaseDL
    {
        protected readonly HttpClient _httpClient;

        /// <summary>
        /// Init HttpDL.
        /// </summary>
        /// <param name="timeout">Set http request timeout.(millisecond)</param>
        /// <param name="proxy">Set http or socks5 proxy.
        /// http://{hostname}:{port} or socks5://{hostname}:{port}</param>
        public HttpDL(int timeout = 60000, string? proxy = null)
            : this(CreateHttpClient(timeout, proxy))
        {
        }

        /// <summary>
        /// Init HttpDL.
        /// </summary>
        /// <param name="httpClient">Set http client.</param>
        public HttpDL(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Download http file.
        /// </summary>
        /// <param name="workDir">Set file download directory.</param>
        /// <param name="saveName">Set file save name.</param>
        /// <param name="url">Set file url.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="threads">Set the number of threads to download.</param>
        /// <param name="delay">Set http request delay.(millisecond)</param>
        /// <param name="maxRetry">Set the maximum number of download retries.</param>
        /// <param name="maxSpeed">Set the maximum download speed.(byte)
        /// 1KB = 1024 byte, 1MB = 1024 * 1024 byte</param>
        /// <param name="interval">Set the progress callback time interval.(millisecond)</param>
        /// <param name="progress">Set progress callback.</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task DownloadAsync(
            string workDir, string saveName, string url, string header = "",
            int threads = 1, int delay = 200, int maxRetry = 20,
            long? maxSpeed = null, int interval = 1000,
            Action<HttpProgressEventArgs>? progress = null,
            CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(workDir))
            {
                throw new Exception("Parameter workDir cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(saveName))
            {
                throw new Exception("Parameter saveName cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new Exception("Parameter url cannot be empty.");
            }

            if (maxSpeed != null && maxSpeed.Value < 1024)
            {
                throw new Exception("Parameter maxSpeed must be greater than or equal to 1024.");
            }

            // saveName = saveName.FilterFileName();

            header = string.IsNullOrWhiteSpace(header) ? "" : header;

            threads = Math.Max(threads, 1);

            var tempDir = Path.Combine(workDir, saveName);
            if (!Directory.Exists(tempDir))
            {
                _ = Directory.CreateDirectory(tempDir);
            }

            var chunkSize = 4 * 1024 * 1024;

            var (respHeaders, _) = await GetHeadersAsync(_httpClient,
                url, header, 0, 0, token);

            var rangeTest = respHeaders?.ContentLength;
            var contentType = respHeaders?.ContentType?.MediaType;
            var contentLength = respHeaders?.ContentRange?.Length;

            var ext = contentType != null ? MimeTypeMap.GetExtension(contentType) : Path.GetExtension(new Uri(url).AbsolutePath);
            bool getResume()
            {
                if (rangeTest != 1)
                {
                    return false;
                }

                return contentLength != null && contentLength > chunkSize;
            }
            var isResume = getResume();

            var retry = 0;
            var downloadBytes = 0L;
            var intervalDownloadBytes = 0L;

            void progressEvent()
            {
                try
                {
                    if (progress != null)
                    {
                        var totalBytes = contentLength;
                        var speed = intervalDownloadBytes / interval * 1000;
                        var percentage = totalBytes != null ?
                            Number.Div(downloadBytes, totalBytes.Value) :
                            null as double?;
                        var eta = totalBytes != null ?
                            (int)Number.Div(totalBytes.Value - downloadBytes, speed) :
                            null as int?;
                        var args = new HttpProgressEventArgs
                        {
                            DownloadBytes = downloadBytes,
                            MaxRetry = maxRetry,
                            Retry = retry,
                            Percentage = percentage,
                            TotalBytes = totalBytes,
                            Speed = speed,
                            Eta = eta
                        };
                        progress(args);
                    }
                }
                catch { }
            }

            async Task<long> copyToAsync(Stream s, Stream d, int threads, CancellationToken token = default)
            {
                var bytes = 0L;
                var buffer = new byte[4096];
                var size = 0;
                var limit = 0L;
                if (maxSpeed != null)
                {
                    limit = (long)((0.001 * interval * maxSpeed.Value) - (threads * 1024));
                }

                while (true)
                {
                    size = await s.ReadAsync(buffer, 0, buffer.Length, token);
                    if (size <= 0)
                    {
                        return bytes;
                    }

                    await d.WriteAsync(buffer, 0, size, token);
                    bytes += size;
                    _ = Interlocked.Add(ref intervalDownloadBytes, size);
                    _ = Interlocked.Add(ref downloadBytes, size);
                    if (maxSpeed != null)
                    {
                        while (intervalDownloadBytes >= limit)
                        {
                            await Task.Delay(1, token);
                        }
                    }
                }
            }

            if (isResume)
            {
                var works = new List<(
                    long index, string filePath, string ext, long from, long to)>();

                List<(long from, long to)> calcChunks()
                {
                    var ranges = new List<(long from, long to)>();

                    var current = 0L;
                    var length = contentLength ?? 0;

                    while (true)
                    {
                        var size = Math.Min(length, chunkSize);
                        if (size == 0)
                        {
                            break;
                        }

                        ranges.Add((current, current + size - 1));
                        current += size;
                        length -= size;
                    }
                    return ranges;
                }
                var chunks = calcChunks();

                var index = 0;
                var count = chunks.Count;
                foreach (var (from, to) in chunks)
                {
                    var chunkExt = ".temp";
                    var fileName = $"{index}".PadLeft($"{count}".Length, '0');
                    var filePath = Path.Combine(tempDir, $"{fileName}");
                    works.Add((index, filePath, chunkExt, from, to));
                    index++;
                }

                var total = 0;
                var finish = 0;
                total = works.Count;

                async Task func()
                {
                    await RetryTask.Run(async (r, ex) =>
                    {
                        finish = 0;
                        downloadBytes = 0L;
                        intervalDownloadBytes = 0L;
                        retry = r;

                        var todoWorks = works
                            .Where(it =>
                            {
                                var savePath = $"{it.filePath}{it.ext}";
                                if (File.Exists(savePath))
                                {
                                    var info = new FileInfo(savePath);
                                    downloadBytes += info.Length;
                                    finish++;
                                    return false;
                                }
                                return true;
                            })
                            .ToList();

                        await ParallelTask.Run(todoWorks, async (it, _token) =>
                        {
                            var index = it.index;
                            var filePath = it.filePath;
                            var chunkExt = it.ext;

                            var rangeFrom = it.from;
                            var rangeTo = it.to;

                            var tempPath = $"{filePath}.downloading";
                            var savePath = $"{filePath}{chunkExt}";

                            _ = await LoadStreamAsync(_httpClient, url, header,
                                async (stream, contentLength, _) =>
                                {
                                    using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                                    {
                                        async Task<Stream> download()
                                        {
                                            var ms = new MemoryStream();
                                            var size = await copyToAsync(stream, ms, threads, _token);
                                            if (size != contentLength)
                                            {
                                                throw new Exception("Chunk size not match content-length.");
                                            }

                                            ms.Position = 0;
                                            return ms;
                                        }
                                        var ms = await download();
                                        await ms.CopyToAsync(fs, 4096, _token);
                                    }
                                    File.Move(tempPath, savePath);
                                }, rangeFrom, rangeTo, _token);
                            finish++;
                        }, threads, delay, token);
                    }, 10 * 1000, maxRetry, token);
                }

                var stop = false;
                var timer = new System.Timers.Timer(interval)
                {
                    AutoReset = true
                };
                timer.Elapsed += delegate
                {
                    if (!stop)
                    {
                        progressEvent();
                        _ = Interlocked.Exchange(ref intervalDownloadBytes, 0);
                    }
                };

                async Task mergeChunks()
                {
                    var chunks = Directory.GetFiles(tempDir)
                        .Where(it => it.EndsWith(".temp")).ToList();

                    if (chunks.Count != works.Count)
                    {
                        throw new Exception("Chunk count not match.");
                    }

                    var outputPath = Path.Combine(tempDir, $"output");

                    using (var fs = new FileStream(
                        $"{outputPath}{ext}", FileMode.Create, FileAccess.Write))
                    {
                        foreach (var chunk in chunks)
                        {
                            using var chunkFs = new FileStream(
                                chunk, FileMode.Open, FileAccess.Read);
                            await chunkFs.CopyToAsync(fs, 4096, token);
                        }
                        if (fs.Length != contentLength)
                        {
                            throw new Exception("File size not match content-length.");
                        }
                    }

                    var finishPath = Path.Combine(workDir, $"{saveName}{ext}");
                    if (File.Exists(finishPath))
                    {
                        finishPath = Path.Combine(workDir,
                            $"{saveName}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}{ext}");
                    }

                    File.Move($"{outputPath}{ext}", finishPath);
                }

                void clear()
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { }
                }

                try
                {
                    timer.Enabled = true;
                    await func();
                    await mergeChunks();
                    clear();
                    stop = true;
                    timer.Enabled = false;
                    progressEvent();

                }
                catch
                {
                    stop = true;
                    timer.Enabled = false;
                    progressEvent();
                    throw;
                }
            }
            // !isResume
            else
            {
                var tempPath = Path.Combine(tempDir, $"temp{ext}");

                async Task func()
                {
                    await RetryTask.Run(async (r, ex) =>
                    {
                        downloadBytes = 0L;
                        intervalDownloadBytes = 0L;
                        retry = r;

                        _ = await LoadStreamAsync(_httpClient, url, header,
                            async (stream, contentLength, _) =>
                            {
                                using var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
                                await copyToAsync(stream, fs, 1, token);
                            }, 0, null, token);
                    }, 10 * 1000, maxRetry, token);
                }

                var stop = false;
                var timer = new System.Timers.Timer(interval)
                {
                    AutoReset = true
                };
                timer.Elapsed += delegate
                {
                    if (!stop)
                    {
                        progressEvent();
                        _ = Interlocked.Exchange(ref intervalDownloadBytes, 0);
                    }
                };

                void finish()
                {
                    if (contentLength != null)
                    {
                        var fileInfo = new FileInfo(tempPath);
                        if (fileInfo.Length != contentLength)
                        {
                            throw new Exception("File size not match content-length.");
                        }
                    }
                    var finishPath = Path.Combine(workDir, $"{saveName}{ext}");
                    if (File.Exists(finishPath))
                    {
                        finishPath = Path.Combine(workDir,
                            $"{saveName}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}{ext}");
                    }

                    File.Move(tempPath, finishPath);
                }

                void clear()
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { }
                }

                try
                {
                    timer.Enabled = true;
                    await func();
                    finish();
                    clear();
                    stop = true;
                    timer.Enabled = false;
                    progressEvent();

                }
                catch
                {
                    stop = true;
                    timer.Enabled = false;
                    progressEvent();
                    throw;
                }
            }
        }
    }
}
