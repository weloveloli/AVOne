// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download.DL
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Providers.Official.Download.Enums;
    using AVOne.Providers.Official.Download.Events;
    using AVOne.Providers.Official.Download.Extensions;
    using AVOne.Providers.Official.Download.Parser;
    using AVOne.Providers.Official.Download.Utils;

    public class HlsDL : BaseDL
    {
        protected readonly HttpClient _httpClient;
        private readonly string _ffmepg;

        /// <summary>
        /// Init HlsDL.
        /// </summary>
        /// <param name="timeout">Set http request timeout.(millisecond)</param>
        /// <param name="proxy">Set http or socks5 proxy.
        /// http://{hostname}:{port} or socks5://{hostname}:{port}</param>
        public HlsDL(string ffmepg = "ffmepg", int timeout = 60000, string? proxy = null)
            : this(CreateHttpClient(timeout, proxy), ffmepg)
        {
        }

        /// <summary>
        /// Init HlsDL.
        /// </summary>
        /// <param name="httpClient">Set http client.</param>
        public HlsDL(HttpClient httpClient, string ffmepg)
        {
            _httpClient = httpClient;
            _ffmepg = ffmepg;
        }

        /// <summary>
        /// Get m3u8 manifest by url.
        /// </summary>
        /// <param name="url">Set m3u8 download url.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        public async Task<(string data, string url)> GetManifestAsync(
            string url, string header = "", CancellationToken token = default)
        {
            return await GetStringAsync(_httpClient, url, header, token);
        }

        /// <summary>
        /// Get m3u8 master playlist by url.
        /// </summary>
        /// <param name="url">Set m3u8 download url.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        public async Task<MasterPlaylist> GetMasterPlaylistAsync(
            string url, string header = "", CancellationToken token = default)
        {
            var manifest = await GetManifestAsync(url, header, token);
            var parser = new MasterPlaylistParser();
            return parser.Parse(manifest.data, manifest.url);
        }

        /// <summary>
        /// Get m3u8 media playlist by url.
        /// </summary>
        /// <param name="url">Set m3u8 download url.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        public async Task<MediaPlaylist> GetMediaPlaylistAsync(
            string url, string header = "", CancellationToken token = default)
        {
            var manifest = await GetManifestAsync(url, header, token);
            var parser = new MediaPlaylistParser();
            return parser.Parse(manifest.data, manifest.url);
        }

        /// <summary>
        /// Parse m3u8 master playlist by manifest.
        /// </summary>
        /// <param name="manifest">Set m3u8 master manifest.</param>
        /// <param name="url">Set m3u8 url.</param>
        /// <returns></returns>
        public MasterPlaylist ParseMasterPlaylist(string manifest, string url = "")
        {
            var parser = new MasterPlaylistParser();
            return parser.Parse(manifest, url);
        }

        /// <summary>
        /// Parse m3u8 media playlist by manifest.
        /// </summary>
        /// <param name="manifest">Set m3u8 media manifest.</param>
        /// <param name="url">Set m3u8 url.</param>
        /// <returns></returns>
        public MediaPlaylist ParseMediaPlaylist(string manifest, string url = "")
        {
            var parser = new MediaPlaylistParser();
            return parser.Parse(manifest, url);
        }

        /// <summary>
        /// Get m3u8 segment keys by parts.
        /// </summary>
        /// <param name="parts">Set m3u8 playlist parts.</param>
        /// <returns></returns>
        public List<SegmentKey> GetKeys(List<Part> parts)
        {
            var keys = new List<SegmentKey>();
            foreach (var part in parts)
            {
                if (part.SegmentMap != null)
                {
                    if (part.SegmentMap.Key.Method != "NONE")
                    {
                        keys.Add(part.SegmentMap.Key);
                    }
                }
                foreach (var item in part.Segments)
                {
                    if (item.Key.Method != "NONE")
                    {
                        keys.Add(item.Key);
                    }
                }
            }
            return keys.Distinct(it => it.Uri).ToList();
        }

        /// <summary>
        /// Get m3u8 key base64 data by segment keys.
        /// </summary>
        /// <param name="segmentKeys">Set m3u8 segment keys.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetKeysDataAsync(
            List<SegmentKey> segmentKeys, string header = "",
            CancellationToken token = default)
        {
            var result = new Dictionary<string, string>();
            foreach (var item in segmentKeys)
            {
                var key = await GetKeyDataAsync(item, header, token);
                result.Add(item.Uri, key);
            }
            return result;
        }

        /// <summary>
        /// Get m3u8 key base64 data by segment key.
        /// </summary>
        /// <param name="segmentKey">Set m3u8 segment key.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        public async Task<string> GetKeyDataAsync(
            SegmentKey segmentKey, string header = "",
            CancellationToken token = default)
        {
            var (data, _) = await GetBytesAsync(_httpClient,
                segmentKey.Uri, header, null, null, token);
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Get first segment.
        /// </summary>
        /// <param name="workDir">Set video download directory.</param>
        /// <param name="saveName">Set video save name.</param>
        /// <param name="parts">Set m3u8 media playlist parts to download.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="keys">Set m3u8 segment keys.</param>
        /// <param name="onSegment">Set segment download callback.</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetFirstSegmentAsync(
            string workDir, string saveName,
            List<Part> parts, string header = "",
            Dictionary<string, string>? keys = null,
            Func<Stream, CancellationToken, Task<Stream>>? onSegment = null,
            CancellationToken token = default)
        {
            await DownloadAsync(
                workDir, saveName, parts, header, keys,
                threads: 1, maxRetry: 0, onlyFirstSegment: true,
                onSegment: onSegment, token: token);

            var tempDir = Path.Combine(workDir, saveName);
            if (!Directory.Exists(tempDir))
            {
                throw new Exception("Not found saveName directory.");
            }

            var partDirs = Directory.GetDirectories(tempDir)
                .Select(it => new
                {
                    Path = it,
                    Name = Path.GetFileName(it)
                })
                .Where(it => it.Name.StartsWith("Part_"))
                .OrderBy(it => it.Path)
                .ToList();

            var firstPath = null as string;

            foreach (var partDir in partDirs)
            {
                var partFiles = Directory.GetFiles(partDir.Path)
                    .Where(it =>
                        it.EndsWith(".mp4") ||
                        it.EndsWith(".m4s") ||
                        it.EndsWith(".ts"))
                    .Select(it => new
                    {
                        item = it,
                        index = Path.GetFileNameWithoutExtension(it)
                            .PadLeft(25, '0')
                    })
                    .OrderBy(it => it.index)
                    .Select(it => it.item)
                    .ToList();

                if (partFiles.Count > 0)
                {
                    firstPath = partFiles.First();
                    break;
                }
            }

            return firstPath == null || !File.Exists(firstPath) ? throw new Exception("Not found first segment.") : firstPath;
        }

        /// <summary>
        /// Download m3u8 ts files.
        /// </summary>
        /// <param name="workDir">Set video download directory.</param>
        /// <param name="saveName">Set video save name.</param>
        /// <param name="parts">Set m3u8 media playlist parts to download.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="keys">Set m3u8 segment keys.</param>
        /// <param name="threads">Set the number of threads to download.</param>
        /// <param name="delay">Set http request delay.(millisecond)</param>
        /// <param name="maxRetry">Set the maximum number of download retries.</param>
        /// <param name="maxSpeed">Set the maximum download speed.(byte)
        /// 1KB = 1024 byte, 1MB = 1024 * 1024 byte</param>
        /// <param name="interval">Set the progress callback time interval.(millisecond)</param>
        /// <param name="checkComplete">Set whether to check file count complete.</param>
        /// <param name="onlyFirstSegment">Set only download the first segment.</param>
        /// <param name="onSegment">Set segment download callback.</param>
        /// <param name="progress">Set progress callback.</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task DownloadAsync(
            string workDir, string saveName,
            List<Part> parts, string header = "",
            Dictionary<string, string>? keys = null,
            int threads = 1, int delay = 200, int maxRetry = 20,
            long? maxSpeed = null, int interval = 1000,
            bool checkComplete = true, bool onlyFirstSegment = false,
            Func<Stream, CancellationToken, Task<Stream>>? onSegment = null,
            Action<ProgressEventArgs>? progress = null,
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

            if (maxSpeed != null && maxSpeed.Value < 1024)
            {
                throw new Exception("Parameter maxSpeed must be greater than or equal to 1024.");
            }

            if (parts == null ||
                parts.Count == 0 || !parts.SelectMany(it => it.Segments).Any())
            {
                throw new Exception("Parameter parts cannot be empty.");
            }

            keys ??= new Dictionary<string, string>();

            // saveName = saveName.FilterFileName();

            header = string.IsNullOrWhiteSpace(header) ? "" : header;

            threads = Math.Max(threads, 1);

            var tempDir = Path.Combine(workDir, saveName);
            if (!Directory.Exists(tempDir))
            {
                _ = Directory.CreateDirectory(tempDir);
            }

            var works = new List<(long index, string filePath, string ext,
                (string Uri, ByteRange? ByteRange, SegmentKey Key) segment)>();

            ((Action)(() =>
            {
                foreach (var part in parts)
                {
                    var count = part.Segments.Count;
                    var partName = $"Part_{part.PartIndex}".PadLeft($"{parts.Count}".Length, '0');
                    var partDir = Path.Combine(tempDir, partName);
                    if (!Directory.Exists(partDir))
                    {
                        _ = Directory.CreateDirectory(partDir);
                    }

                    var hasMap = false;
                    if (part.SegmentMap != null)
                    {
                        var mapName = "!MAP";
                        var mapPath = Path.Combine(partDir, mapName);
                        var mapIndex = part.Segments.Count == 0 ?
                            0 : part.Segments.First().Index;
                        works.Add((mapIndex, mapPath, ".mp4",
                            (part.SegmentMap.Uri,
                             part.SegmentMap.ByteRange,
                             part.SegmentMap.Key)));
                        hasMap = true;
                        if (onlyFirstSegment)
                        {
                            return;
                        }
                    }

                    foreach (var item in part.Segments)
                    {
                        var ext = hasMap ? ".m4s" : ".ts";
                        var fileName = $"{item.Index}".PadLeft($"{count}".Length, '0');
                        var filePath = Path.Combine(partDir, $"{fileName}");
                        works.Add((item.Index, filePath, ext,
                            (item.Uri, item.ByteRange, item.Key)));
                        if (onlyFirstSegment)
                        {
                            return;
                        }
                    }
                }
            }))();

            var retry = 0;
            var total = 0;
            var finish = 0;
            var downloadBytes = 0L;
            var intervalDownloadBytes = 0L;
            total = works.Count;

            async Task<long> copyToAsync(Stream s, Stream d,
                CancellationToken token = default)
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
                    if (maxSpeed != null)
                    {
                        while (intervalDownloadBytes >= limit)
                        {
                            await Task.Delay(1, token);
                        }
                    }
                }
            }

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

                    await ParallelTask.Run(todoWorks, async (item, _token) =>
                    {
                        var index = item.index;
                        var filePath = item.filePath;
                        var ext = item.ext;
                        var segment = item.segment;

                        var rangeFrom = null as long?;
                        var rangeTo = null as long?;
                        if (segment.ByteRange != null)
                        {
                            rangeFrom = segment.ByteRange.Offset ?? 0;
                            rangeTo = rangeFrom + segment.ByteRange.Length - 1;
                        }

                        var tempPath = $"{filePath}.downloading";
                        var savePath = $"{filePath}{ext}";

                        _ = await LoadStreamAsync(_httpClient, segment.Uri, header,
                            async (stream, contentLength, _) =>
                            {
                                using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                                {
                                    async Task<Stream> download()
                                    {
                                        var ms = new MemoryStream();
                                        var size = await copyToAsync(stream, ms, _token);
                                        if (contentLength != null)
                                        {
                                            if (size != contentLength)
                                            {
                                                throw new Exception("Segment size not match content-length.");
                                            }
                                        }
                                        Interlocked.Add(ref downloadBytes, size);
                                        ms.Position = 0;
                                        return ms;
                                    }

                                    if (segment.Key.Method != "NONE")
                                    {
                                        if (!keys.TryGetValue(segment.Key.Uri, out var key))
                                        {
                                            throw new Exception("Not found segment key.");
                                        }

                                        var iv = segment.Key.IV;

                                        var ms = await download();
                                        if (onSegment != null)
                                        {
                                            var decryptStream = new MemoryStream();
                                            await new Cryptor().AES128Decrypt(
                                                ms, key, iv, decryptStream, _token);
                                            decryptStream.Position = 0;
                                            ms = await onSegment(decryptStream, _token);
                                            await ms.CopyToAsync(fs, 4096, _token);
                                        }
                                        else
                                        {
                                            await new Cryptor().AES128Decrypt(
                                                ms, key, iv, fs, _token);
                                        }
                                    }
                                    else
                                    {
                                        var ms = await download();
                                        if (onSegment != null)
                                        {
                                            ms = await onSegment(ms, _token);
                                            await ms.CopyToAsync(fs, 4096, _token);
                                        }
                                        else
                                        {
                                            await ms.CopyToAsync(fs, 4096, _token);
                                        }
                                    }
                                }
                                File.Move(tempPath, savePath);
                            }, rangeFrom, rangeTo, _token);
                        finish++;
                    }, threads, delay, token);
                }, 10 * 1000, maxRetry, token);
            }

            void progressEvent()
            {
                try
                {
                    if (progress != null)
                    {
                        var percentage = Number.Div(finish, total);
                        var totalBytes = (long)Number.Div(downloadBytes * total, finish);
                        var speed = intervalDownloadBytes / interval * 1000;
                        var eta = (int)Number.Div(totalBytes - downloadBytes, speed);
                        var args = new ProgressEventArgs
                        {
                            Total = total,
                            Finish = finish,
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

            void checkFilesComplete()
            {
                var count = 0;
                var partDirs = Directory.GetDirectories(tempDir);
                foreach (var partDir in partDirs)
                {
                    var partDirName = Path.GetFileName(partDir);
                    if (!partDirName.StartsWith("Part_"))
                    {
                        continue;
                    }

                    var partFiles = Directory.GetFiles(partDir)
                        .Where(it =>
                            it.EndsWith(".mp4") ||
                            it.EndsWith(".m4s") ||
                            it.EndsWith(".ts"));
                    count += partFiles.Count();
                }
                if (count != works.Count)
                {
                    throw new Exception("Segment count not match.");
                }
            }

            try
            {
                timer.Enabled = true;
                await func();
                if (checkComplete && !onlyFirstSegment)
                {
                    checkFilesComplete();
                }

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

        /// <summary>
        /// Merge m3u8 ts files.
        /// </summary>
        /// <param name="workDir">Set video download directory.</param>
        /// <param name="saveName">Set video save name.</param>
        /// <param name="outputFormat">Set video output format.</param>
        /// <param name="binaryMerge">Set use binary merge.</param>
        /// <param name="keepFragmented">Set keep fragmented mp4.</param>
        /// <param name="discardcorrupt">Set ffmpeg discard corrupted packets.</param>
        /// <param name="genpts">Set ffmpeg generate missing PTS if DTS is present.</param>
        /// <param name="igndts">Set ffmpeg ignore DTS if PTS is set.</param>
        /// <param name="ignidx">Set ffmpeg ignore index.</param>
        /// <param name="clearTempFile">Set whether to clear the temporary file after the merge is completed.</param>
        /// <param name="onMessage">Set callback function for FFmpeg warning or error messages.</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> MergeAsync(string workDir, string saveName,
            OutputFormat outputFormat = OutputFormat.MP4, bool binaryMerge = false,
            bool keepFragmented = false, bool discardcorrupt = false,
            bool genpts = false, bool igndts = false, bool ignidx = false,
            bool clearTempFile = false, Action<string>? onMessage = null,
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

            // saveName = saveName.FilterFileName();

            var tempDir = Path.Combine(workDir, saveName);
            if (!Directory.Exists(tempDir))
            {
                throw new Exception("Not found saveName directory.");
            }

            var parts = Directory.GetDirectories(tempDir)
                .Select(it => new
                {
                    Path = it,
                    Name = Path.GetFileName(it)
                })
                .Where(it => it.Name.StartsWith("Part_"))
                .OrderBy(it => it.Path)
                .ToList();

            if (parts.Count == 0)
            {
                throw new Exception("Directory parts cannot be empty.");
            }

            foreach (var part in parts)
            {
                var hasMap = File.Exists(
                    Path.Combine(part.Path, "!MAP.mp4"));

                var partFiles = Directory.GetFiles(part.Path)
                    .Where(it =>
                        it.EndsWith(".mp4") ||
                        it.EndsWith(".m4s") ||
                        it.EndsWith(".ts"))
                    .Select(it => new
                    {
                        item = it,
                        index = Path.GetFileNameWithoutExtension(it)
                            .PadLeft(25, '0')
                    })
                    .OrderBy(it => it.index)
                    .Select(it => it.item)
                    .ToList();

                if (partFiles.Count == 0)
                {
                    continue;
                }

                var partOutputPath = Path.Combine(tempDir, $"{part.Name}");

                var format = hasMap ? "fmp4" : "ts";

                var _binaryMerge = binaryMerge;
                if (format == "fmp4" || parts.Count > 1 || partFiles.Count > 1800)
                {
                    _binaryMerge = true;
                }

                if (_binaryMerge)
                {
                    var ext = format == "fmp4" ? ".fmp4" : ".ts";

                    using (var fs = new FileStream(
                        $"{partOutputPath}{ext}", FileMode.Create, FileAccess.Write))
                    {
                        foreach (var partFile in partFiles)
                        {
                            using var tempFs = new FileStream(
                                partFile, FileMode.Open, FileAccess.Read);
                            await tempFs.CopyToAsync(fs, 4096, token);
                        }
                    }

                    if (format == "fmp4" && !keepFragmented)
                    {
                        var arguments = "";
                        arguments += $@"-loglevel warning -f mp4 -i ""{partOutputPath}{ext}"" ";
                        arguments += $@"-c copy -y -f mp4 ""{partOutputPath}.mp4""";
                        await FFmpeg.ExecuteAsync(arguments, _ffmepg, null, onMessage, token);
                        File.Delete($"{partOutputPath}{ext}");
                    }
                    else
                    {
                        if (File.Exists($"{partOutputPath}.mp4"))
                        {
                            File.Delete($"{partOutputPath}.mp4");
                        }

                        File.Move($"{partOutputPath}{ext}", $"{partOutputPath}.mp4");
                    }
                }
                else
                {
                    var concatText = string.Join("|",
                        partFiles.Select(it => Path.GetFileName(it)));

                    var arguments = "";
                    var fflags = "";
                    fflags += discardcorrupt ? "+discardcorrupt" : "";
                    fflags += genpts ? "+genpts" : "";
                    fflags += igndts ? "+igndts" : "";
                    fflags += ignidx ? "+ignidx" : "";
                    fflags = fflags != "" ? $"-fflags {fflags}" : "";
                    if (outputFormat is not OutputFormat.MP4 and
                        not OutputFormat.TS and
                        not OutputFormat.M4A)
                    {
                        fflags = "";
                    }
                    arguments += $@"-loglevel warning {fflags} -i concat:""{concatText}"" ";

                    arguments += outputFormat switch
                    {
                        OutputFormat.MP4 => $@"-map 0:v? -map 0:a? -map 0:s? -c copy -y -bsf:a aac_adtstoasc ""{partOutputPath}.mp4""",
                        OutputFormat.TS => $@"-map 0 -c copy -y -f mpegts -bsf:v h264_mp4toannexb ""{partOutputPath}.ts""",
                        OutputFormat.M4A => $@"-map 0:a -c copy -y ""{partOutputPath}.m4a""",
                        OutputFormat.SRT => $@"-map 0 -y ""{partOutputPath}.srt""",
                        _ => throw new Exception("OutputFormat not match."),
                    };
                    var workingDir = Path.GetDirectoryName(partFiles.First());
                    await FFmpeg.ExecuteAsync(arguments, _ffmepg, workingDir, onMessage, token);
                }
            }

            void clear()
            {
                if (clearTempFile)
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { }
                }
            }

            string getFormatExt()
            {
                if (outputFormat == OutputFormat.MP4)
                {
                    return ".mp4";
                }

                if (outputFormat == OutputFormat.TS)
                {
                    return ".ts";
                }

                return outputFormat == OutputFormat.M4A ? ".m4a" : outputFormat == OutputFormat.SRT ? ".srt" : "";
            }

            var files = Directory.GetFiles(tempDir)
                .Where(it =>
                    it.EndsWith(".mp4") ||
                    it.EndsWith(".ts") ||
                    it.EndsWith(".m4a") ||
                    it.EndsWith(".srt"))
                .OrderBy(it => it)
                .ToList();

            if (files.Count == 0)
            {
                clear();
                return "";
            }

            if (files.Count == 1)
            {
                var file = files.First();
                var ext = Path.GetExtension(file);
                if (ext == getFormatExt())
                {
                    var finishPath = Path.Combine(workDir, $"{saveName}{ext}");
                    if (File.Exists(finishPath))
                    {
                        finishPath = Path.Combine(workDir,
                            $"{saveName}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}{ext}");
                    }

                    File.Move(file, finishPath);
                    clear();
                    return finishPath;
                }
            }

            // Multiple or format
            {
                var outputPath = Path.Combine(tempDir, $"output");
                var concatTextPath = Path.Combine(tempDir, $"concat.txt");
                var manifest = files
                    .Aggregate(new StringBuilder(),
                        (r, it) => r.AppendLine($@"file '{it}'")).ToString();
                File.WriteAllText(concatTextPath, manifest);

                var arguments = "";
                var fflags = "";
                fflags += discardcorrupt ? "+discardcorrupt" : "";
                fflags += genpts ? "+genpts" : "";
                fflags += igndts ? "+igndts" : "";
                fflags += ignidx ? "+ignidx" : "";
                fflags = fflags != "" ? $"-fflags {fflags}" : "";
                if (outputFormat is not OutputFormat.MP4 and
                    not OutputFormat.TS and
                    not OutputFormat.M4A)
                {
                    fflags = "";
                }
                arguments += $@"-loglevel warning {fflags} -f concat -safe 0 -i ""{concatTextPath}"" ";

                switch (outputFormat)
                {
                    case OutputFormat.MP4:
                        var aacFilter = files.Any(it => it.EndsWith(".ts")) ? "-bsf:a aac_adtstoasc" : "";
                        arguments += $@"-map 0:v? -map 0:a? -map 0:s? -c copy -y {aacFilter} ""{outputPath}.mp4""";
                        break;
                    case OutputFormat.TS:
                        arguments += $@"-map 0 -c copy -y -f mpegts -bsf:v h264_mp4toannexb ""{outputPath}.ts""";
                        break;
                    case OutputFormat.M4A:
                        arguments += $@"-map 0:a -c copy -y ""{outputPath}.m4a""";
                        break;
                    case OutputFormat.SRT:
                        arguments += $@"-map 0 -y ""{outputPath}.srt""";
                        break;
                    default:
                        throw new Exception("OutputFormat not match.");
                }

                await FFmpeg.ExecuteAsync(arguments, _ffmepg, null, onMessage, token);
                File.Delete(concatTextPath);

                var output = Directory.GetFiles(tempDir)
                    .Where(it => Path.GetFileName(it).StartsWith("output"))
                    .First();

                var ext = Path.GetExtension(output);
                var finishPath = Path.Combine(workDir, $"{saveName}{ext}");
                if (File.Exists(finishPath))
                {
                    finishPath = Path.Combine(workDir,
                        $"{saveName}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}{ext}");
                }

                File.Move(output, finishPath);

                foreach (var file in files)
                {
                    File.Delete(file);
                }

                clear();
                return finishPath;
            }
        }

        /// <summary>
        /// REC m3u8 live stream.
        /// </summary>
        /// <param name="workDir">Set video download directory.</param>
        /// <param name="saveName">Set video save name.</param>
        /// <param name="url">Set m3u8 live stream url.</param>
        /// <param name="header">Set http request header.
        /// format: key1:key1|key2:key2</param>
        /// <param name="maxRetry">Set the maximum number of download retries.</param>
        /// <param name="maxSpeed">Set the maximum download speed.(byte)
        /// 1KB = 1024 byte, 1MB = 1024 * 1024 byte</param>
        /// <param name="interval">Set the progress callback time interval.(millisecond)</param>
        /// <param name="noSegStopTime">Set how long to stop after when there is no segment.(millisecond)</param>
        /// <param name="onSegment">Set segment download callback.</param>
        /// <param name="progress">Set progress callback.</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        public async Task REC(
            string workDir, string saveName,
            string url, string header = "", int maxRetry = 20,
            long? maxSpeed = null, int interval = 1000, int? noSegStopTime = null,
            Func<Stream, CancellationToken, Task<Stream>>? onSegment = null,
            Action<RecProgressEventArgs>? progress = null,
            CancellationToken token = default)
        {
            var finishDict = new Dictionary<long, bool>();
            var keys = new Dictionary<string, string>();

            var retry = 0;
            var now = DateTime.Now;
            var sw = new Stopwatch();

            var finish = 0;
            var downloadBytes = 0L;
            var todoFinish = 0;
            var todoDownloadBytes = 0L;
            var speed = 0L;
            var noSegDuration = 0;
            var currentIndex = long.MaxValue * -1;
            var currentPartIndex = int.MaxValue * -1;
            var startIndex = null as long?;
            var lost = 0;

            void progressEvent()
            {
                try
                {
                    if (progress != null)
                    {
                        var args = new RecProgressEventArgs
                        {
                            Finish = finish + todoFinish,
                            DownloadBytes = downloadBytes + todoDownloadBytes,
                            MaxRetry = maxRetry,
                            Retry = retry,
                            Speed = speed,
                            Lost = lost,
                            RecTime = DateTime.Now - now
                        };
                        progress(args);
                    }
                }
                catch { }
            }

            async Task func()
            {
                await RetryTask.Run(async (r, ex) =>
                {
                    todoFinish = 0;
                    todoDownloadBytes = 0;
                    retry = r;

                    while (true)
                    {
                        sw.Restart();

                        // Download and parse m3u8 manifest
                        var mediaPlaylist = await GetMediaPlaylistAsync(url, header, token);

                        // Download m3u8 segment key
                        var segmentKeys = GetKeys(mediaPlaylist.Parts);
                        foreach (var segmentKey in segmentKeys)
                        {
                            if (!keys.ContainsKey(segmentKey.Uri))
                            {
                                var key = await GetKeyDataAsync(segmentKey, header, token);
                                keys.Add(segmentKey.Uri, key);
                            }
                        }

                        // Get todo part list
                        var todoParts = mediaPlaylist.Parts
                            .Select(it =>
                            {
                                var segs = it.Segments
                                    .Where(itt => !finishDict.ContainsKey(itt.Index))
                                    .ToList();
                                it.Segments = segs;
                                return it;
                            })
                            .Where(it => it.Segments.Count > 0)
                            .ToList();

                        // Check playlist reset
                        bool isReset()
                        {
                            foreach (var part in todoParts)
                            {
                                if (part.PartIndex < currentPartIndex)
                                {
                                    return true;
                                }

                                if (part.PartIndex == currentPartIndex)
                                {
                                    foreach (var seg in part.Segments)
                                    {
                                        if (seg.Index < currentIndex)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            return false;
                        }
                        if (isReset())
                        {
                            break;
                        }

                        var hasSeg = false;
                        if (todoParts.Count > 0)
                        {
                            hasSeg = true;
                            noSegDuration = 0;

                            // Download m3u8 ts files
                            await DownloadAsync(workDir,
                                saveName, todoParts, header, keys,
                                threads: 1, delay: 1, maxRetry: 0,
                                maxSpeed: maxSpeed, checkComplete: false,
                                interval: interval, onSegment: onSegment,
                                progress: (args) =>
                                {
                                    speed = args.Speed;
                                    todoFinish = args.Finish;
                                    todoDownloadBytes = args.DownloadBytes;
                                },
                                token: token);
                            sw.Stop();

                            // Update parameters
                            finish += todoFinish;
                            downloadBytes += todoDownloadBytes;
                            todoFinish = 0;
                            todoDownloadBytes = 0;
                            todoParts
                                .SelectMany(it => it.Segments)
                                .ToList()
                                .ForEach(it =>
                                {
                                    if (!finishDict.ContainsKey(it.Index))
                                    {
                                        finishDict.Add(it.Index, true);
                                    }
                                });

                            // Update index
                            var lastPart = todoParts.Last();
                            currentPartIndex = lastPart.PartIndex;
                            currentIndex = lastPart.Segments.Last().Index;
                        }

                        // Check lost
                        startIndex ??= todoParts
                                .FirstOrDefault()?.Segments
                                .FirstOrDefault()?.Index;
                        if (startIndex != null)
                        {
                            lost = (int)(currentIndex - startIndex.Value - finishDict.Count + 1);
                        }

                        // Live ends
                        if (mediaPlaylist.EndList)
                        {
                            break;
                        }

                        if (noSegStopTime != null)
                        {
                            if (noSegDuration >= noSegStopTime.Value)
                            {
                                break;
                            }
                        }

                        // Waiting for new segment
                        var targetDurationTime =
                            mediaPlaylist.TargetDuration * 1000;

                        if (!hasSeg)
                        {
                            var half = targetDurationTime / 2;
                            noSegDuration += half;
                            var timespan = half - sw.ElapsedMilliseconds;
                            if (timespan > 0)
                            {
                                await Task.Delay(half, token);
                                continue;
                            }
                        }
                        else
                        {
                            var timespan = targetDurationTime - sw.ElapsedMilliseconds;
                            if (timespan > 0)
                            {
                                await Task.Delay((int)timespan, token);
                                continue;
                            }
                        }
                        await Task.Delay(1);
                    }
                }, 1, maxRetry, token);
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
                }
            };

            try
            {
                timer.Enabled = true;
                await func();
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

        /// <summary>
        /// Muxing video source and audio source
        /// </summary>
        /// <param name="workDir">Set video save directory.</param>
        /// <param name="saveName">Set video save name.</param>
        /// <param name="videoSourcePath">Set video source path.</param>
        /// <param name="audioSourcePath">Set audio source path.</param>
        /// <param name="outputFormat">Set video output format.</param>
        /// <param name="clearSource">Set whether to clear source file after the muxing is completed.</param>
        /// <param name="onMessage">Set callback function for FFmpeg warning or error messages.</param>
        /// <param name="token">Set cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task MuxingAsync(
            string workDir, string saveName,
            string videoSourcePath, string audioSourcePath,
            MuxOutputFormat outputFormat = MuxOutputFormat.MP4,
            bool clearSource = false, Action<string>? onMessage = null,
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

            if (string.IsNullOrWhiteSpace(videoSourcePath))
            {
                throw new Exception("Parameter videoSourcePath cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(audioSourcePath))
            {
                throw new Exception("Parameter audioSourcePath cannot be empty.");
            }

            // saveName = saveName.FilterFileName();

            if (!File.Exists(videoSourcePath))
            {
                throw new Exception("Not found video source.");
            }

            if (!File.Exists(audioSourcePath))
            {
                throw new Exception("Not found audio source.");
            }

            var tempPath = Path.Combine(workDir, $"{saveName}.muxing");
            if (File.Exists(tempPath))
            {
                tempPath = Path.Combine(workDir,
                    $"{saveName}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.muxing");
            }

            var arguments = "";
            arguments += $@"-loglevel warning -i ""{videoSourcePath}"" -i ""{audioSourcePath}"" ";

            switch (outputFormat)
            {
                case MuxOutputFormat.MP4:
                    var aacFilter = videoSourcePath.EndsWith(".ts") || audioSourcePath.EndsWith(".ts") ? "-bsf:a aac_adtstoasc" : "";
                    arguments += $@"-acodec copy -vcodec copy -y -f mp4 {aacFilter} ""{tempPath}""";
                    break;
                case MuxOutputFormat.TS:
                    arguments += $@"-acodec copy -vcodec copy -y -f mpegts -bsf:v h264_mp4toannexb ""{tempPath}""";
                    break;
                default:
                    throw new Exception("OutputFormat not match.");
            }

            await FFmpeg.ExecuteAsync(arguments, _ffmepg, null, onMessage, token);

            var ext = outputFormat == MuxOutputFormat.MP4 ? ".mp4" : ".ts";
            var finishPath = Path.Combine(workDir, $"{saveName}{ext}");
            if (File.Exists(finishPath))
            {
                finishPath = Path.Combine(workDir,
                    $"{saveName}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}{ext}");
            }

            File.Move(tempPath, finishPath);

            if (clearSource)
            {
                File.Delete(videoSourcePath);
                File.Delete(audioSourcePath);
            }
        }
    }
}
