// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.M3U8
{
    using System;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Enum;
    using AVOne.Configuration;
    using AVOne.Constants;
    using AVOne.Enum;
    using AVOne.Extensions;
    using AVOne.Models.Download;
    using AVOne.Models.Job;
    using AVOne.Providers.Download;
    using AVOne.Providers.Official.Downloader.M3U8.Parser;
    using AVOne.Providers.Official.Downloader.M3U8.Utils;
    using Furion.FriendlyException;
    using Furion.Localization;
    using Microsoft.Extensions.Logging;

    public class M3U8DownloadProvider : IDownloaderProvider
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly IStartupOptions _options;
        private readonly HttpClient _client;
        private readonly IConfigurationManager _configurationManager;

        private readonly ILogger<M3U8DownloadProvider> logger;

        public M3U8DownloadProvider(ILogger<M3U8DownloadProvider> logger, IApplicationPaths applicationPaths, IStartupOptions options, IHttpClientFactory httpClientFactory, IConfigurationManager configurationManager)
        {
            _applicationPaths = applicationPaths;
            _options = options;
            _client = httpClientFactory.CreateClient(HttpClientNames.Download);
            _configurationManager = configurationManager;
            this.logger = logger;
        }

        public string Name => "Official";

        public int Order => 1;

        public string DisplayName => L.Text["Official M3U8 Donwloader"];

        public async Task CreateTask(BaseDownloadableItem item, DownloadOpts opts, CancellationToken token = default)
        {
            if (item is not M3U8Item m3U8Item || string.IsNullOrEmpty(item.SaveName) || string.IsNullOrEmpty(m3U8Item.Url))
            {
                throw Oops.Oh(ErrorCodes.INVALID_DOWNLOADABLE_ITEM);
            }

            var saveName = opts.PreferName ?? item.SaveName!;
            var url = m3U8Item.Url!;
            var md5 = url.GetMD5();
            var threadCount = opts.ThreadCount ?? 4;
            var retryCount = opts.RetryCount ?? 3;
            var saveDir = opts.OutputDir ?? Directory.GetCurrentDirectory();
            var tmpDir = _applicationPaths.CachePath;
            var workingDir = Path.Combine(tmpDir, $"{saveName}-{md5}");
            Directory.CreateDirectory(workingDir);
            var playMetaDataFile = $"{saveName}-{md5}.meta.json";

            // check if playList exists
            MediaPlaylist? mediaPlaylist = null;
            if (File.Exists(Path.Combine(workingDir, playMetaDataFile)))
            {
                mediaPlaylist = JsonSerializer.Deserialize<MediaPlaylist>(File.ReadAllText(Path.Combine(workingDir, playMetaDataFile)));
            }

            if (mediaPlaylist is null)
            {
                mediaPlaylist = await GetMediaPlaylist(workingDir, url, saveName, m3U8Item, token);
                File.WriteAllText(Path.Combine(workingDir, playMetaDataFile), JsonSerializer.Serialize(mediaPlaylist));
            }

            if (mediaPlaylist is null)
            {
                throw Oops.Oh(ErrorCodes.INVALID_DOWNLOADABLE_ITEM);
            }

            DownloadParts(workingDir, mediaPlaylist, opts, m3U8Item, threadCount, token);
            opts.OnStatusChanged(new JobStatusArgs { Status = "Merge media" });
            logger.LogDebug($"Start merging file");
            var finalPath = await MergeAsync(workingDir, saveDir, saveName, OutputFormat.MP4, token: token);
            logger.LogDebug($"Downloaded file: {finalPath}");
            var finalFileInfo = new FileInfo(finalPath);

            opts.OnStatusChanged(new DownloadFinishEventArgs { Status = "Download finished", FinalFilePath = finalPath, Progress = 100 });
            Directory.Delete(workingDir, true);
        }

        private void DownloadParts(string workingDir, MediaPlaylist mediaPlaylist, DownloadOpts opts, M3U8Item m3U8Item, int threadCount, CancellationToken token)
        {
            logger.LogDebug($"DownloadParts");
            int interval = 1000;
            var downloadBytes = 0L;
            var intervalDownloadBytes = 0L;
            var total = mediaPlaylist.Parts.Select(e => e.Segments.Count).Sum();
            var stop = false;
            var finish = 0;
            var timer = new System.Timers.Timer(interval)
            {
                AutoReset = true
            };
            timer.Elapsed += delegate
            {
                if (!stop)
                {
                    ProgressEvent(opts, interval, downloadBytes, intervalDownloadBytes, total, finish);
                    Interlocked.Exchange(ref intervalDownloadBytes, 0);
                }
            };
            timer.Enabled = true;

            try
            {
                foreach (var (part, index) in mediaPlaylist.Parts.Select((e, i) => (e, i)))
                {
                    var partDir = Path.Combine(workingDir, $"Part_{index}");
                    Directory.CreateDirectory(partDir);
                    var list = mediaPlaylist.Parts[index].Segments;
                    var numbers = Enumerable.Range(0, list.Count).ToList();

                    var keyDict = new Dictionary<string, byte[]>();
                    var keyList = list.Where(e => e.Key.Method != "NONE").Select(e => e.Key.Uri).Distinct();

                    foreach (var key in keyList)
                    {
                        var request = new HttpRequestMessage
                        {
                            RequestUri = new Uri(key),
                        };

                        if (m3U8Item.Header != null)
                        {
                            foreach (var header in m3U8Item.Header)
                            {
                                request.Headers.Add(header.Key, header.Value);
                            }
                        }

                        var rsp = _client.Send(request);
                        var bytes = rsp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                        keyDict[key] = bytes;

                    }

                    _ = Parallel.ForEach(numbers, new ParallelOptions { MaxDegreeOfParallelism = threadCount, CancellationToken = token }, index =>
                    {
                        var i = 1;
                        while (i < opts.RetryCount)
                        {
                            try
                            {
                                var segment = list[index];
                                var tsPath = Path.Combine(partDir, $"{index}.ts");
                                var tsTmpPath = Path.Combine(partDir, $"{index}.tmp");
                                if (!File.Exists(tsPath))
                                {
                                    var request = new HttpRequestMessage
                                    {
                                        RequestUri = new Uri(segment.Uri),
                                    };

                                    if (m3U8Item.Header != null)
                                    {
                                        foreach (var header in m3U8Item.Header)
                                        {
                                            request.Headers.Add(header.Key, header.Value);
                                        }
                                    }

                                    var rsp = _client.Send(request);
                                    rsp.EnsureSuccessStatusCode();

                                    var stream = File.OpenWrite(tsTmpPath);
                                    using var tsStream = rsp.Content.ReadAsStream();
                                    CopyTo(tsStream, stream, token, opts, interval, ref intervalDownloadBytes);
                                    stream.Flush();
                                    stream.Close();
                                    if (segment.Key.Method == "AES-128")
                                    {
                                        using var aesAlg = Aes.Create();
                                        aesAlg.Key = keyDict[segment.Key.Uri];
                                        aesAlg.IV = ConvertHexStringToByteArray(segment.Key.IV);
                                        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                                        using var fsCrypt = new FileStream(tsTmpPath, FileMode.Open);
                                        using var cs = new CryptoStream(fsCrypt, decryptor, CryptoStreamMode.Read);
                                        using var fsOut = new FileStream(tsPath, FileMode.Create);
                                        int read;
                                        var buffer = new byte[1048576];
                                        while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            fsOut.Write(buffer, 0, read);
                                        }
                                    }
                                    else
                                    {
                                        File.Move(tsTmpPath, tsPath);
                                    }
                                    var info = new FileInfo(tsPath);
                                    Interlocked.Add(ref downloadBytes, info.Length);
                                }
                                else
                                {
                                    var info = new FileInfo(tsPath);
                                    Interlocked.Add(ref downloadBytes, info.Length);
                                }
                                var done = Interlocked.Increment(ref finish);
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(opts.RetryWait ?? 1000);
                                if (i == opts.RetryCount)
                                {
                                    stop = true;
                                    timer.Enabled = false;
                                    throw;
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Download Parts Failed");
                throw;
            }
        }
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Invalid length of hex string.");
            }
            if (hexString.StartsWith("0x"))
            {
                hexString = hexString.Substring(2);
            }

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }
        private static long CopyTo(Stream s, Stream d, CancellationToken token, DownloadOpts opts, int interval, ref long intervalDownloadBytes)
        {
            var bytes = 0L;
            var buffer = new byte[4096];
            var size = 0;
            var limit = 0L;
            if (opts.MaxSpeed != null && opts.MaxSpeed > 0 && opts.ThreadCount > 0)
            {
                limit = (long)((0.001 * interval * opts.MaxSpeed!) - opts.ThreadCount * 1024);
            }

            while (true)
            {
                size = s.Read(buffer, 0, buffer.Length);
                if (size <= 0)
                    return bytes;
                d.Write(buffer, 0, size);
                bytes += size;
                Interlocked.Add(ref intervalDownloadBytes, size);
                if (opts.MaxSpeed != null)
                {
                    while (intervalDownloadBytes >= limit)
                    {
                        Task.Delay(1, token);
                    }
                }
            }
        }

        private static void ProgressEvent(DownloadOpts opts, int interval, long downloadBytes, long intervalDownloadBytes, int total, int finish)
        {
            try
            {
                var percentage = Div(finish, total);
                var totalBytes = (long)Div(downloadBytes * total, finish);
                var speed = intervalDownloadBytes / interval * 1000;
                var eta = (int)Div(totalBytes - downloadBytes, speed);
                var args = new DownloadProgressEventArgs
                {
                    Total = total,
                    Finish = finish,
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

        /// <summary>
        /// Gets the media playlist.
        /// </summary>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="url">The URL.</param>
        /// <param name="saveName">Name of the save.</param>
        /// <param name="m3U8Item">The m3 u8 item.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private async Task<MediaPlaylist> GetMediaPlaylist(string workingDir, string url, string saveName, M3U8Item m3U8Item, CancellationToken token)
        {
            var md5 = url.GetMD5();
            var playListName = $"{saveName}-{md5}.m3u8";
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
            };

            if (m3U8Item.Header != null)
            {

                foreach (var header in m3U8Item.Header)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var rsp = await _client.SendAsync(request, token);
            rsp.EnsureSuccessStatusCode();
            var m3u8 = await rsp.Content.ReadAsStringAsync(token);
            if (string.IsNullOrEmpty(m3u8))
            {
                throw Oops.Oh(ErrorCodes.INVALID_DOWNLOADABLE_ITEM);
            }
            File.WriteAllText(Path.Combine(workingDir, playListName), m3u8);
            var parser = new MediaPlaylistParser();
            var parsed = parser.Parse(m3u8, url);

            /// master playlist
            if (parsed.Parts.Count == 1 && parsed.Parts[0].Segments.All(e => e.Uri.EndsWith("m3u8")))
            {
                logger.LogInformation($"Get master playlist: {url}");
                return await GetMediaPlaylist(workingDir, parsed.Parts[0].Segments[0].Uri, saveName, m3U8Item, token);
            }
            else
            {
                return parsed;
            }
        }

        /// <summary>
        /// Merge m3u8 ts files.
        /// </summary>
        /// <param name="workDir">Set video working directory.</param>
        /// <param name="outputDir">Set video download directory.</param>
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
        public async Task<string> MergeAsync(string workDir, string outputDir, string saveName,
            OutputFormat outputFormat = OutputFormat.MP4, bool binaryMerge = false,
            bool keepFragmented = false, bool discardcorrupt = false,
            bool genpts = false, bool igndts = false, bool ignidx = false, Action<string>? onMessage = null,
            CancellationToken token = default)
        {
            var ffmpegPath = _options.FFmpegPath ?? _configurationManager.CommonConfiguration.FFmpegConfig.FFmpegPath ?? "ffmepge";
            if (string.IsNullOrWhiteSpace(workDir))
                throw new Exception("Parameter workDir cannot be empty.");
            if (string.IsNullOrWhiteSpace(saveName))
                throw new Exception("Parameter saveName cannot be empty.");

            if (!Directory.Exists(workDir))
                throw new Exception("Not found saveName directory.");

            var parts = Directory.GetDirectories(workDir)
                .Select(it => new
                {
                    Path = it,
                    Name = Path.GetFileName(it)
                })
                .Where(it => it.Name.StartsWith("Part_"))
                .OrderBy(it => it.Path)
                .ToList();

            if (parts.Count == 0)
                throw new Exception("Directory parts cannot be empty.");

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
                    continue;

                var partOutputPath = Path.Combine(workDir, $"{part.Name}");

                var format = hasMap ? "fmp4" : "ts";

                var _binaryMerge = binaryMerge;
                if (format == "fmp4" || parts.Count > 1 || partFiles.Count > 1800)
                    _binaryMerge = true;

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
                        await FFmpeg.ExecuteAsync(ffmpegPath, arguments, null, onMessage, token);
                        File.Delete($"{partOutputPath}{ext}");
                    }
                    else
                    {
                        if (File.Exists($"{partOutputPath}.mp4"))
                            File.Delete($"{partOutputPath}.mp4");
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
                    if (outputFormat != OutputFormat.MP4 &&
                        outputFormat != OutputFormat.TS &&
                        outputFormat != OutputFormat.M4A)
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
                    logger.LogDebug("arguments:{0}", arguments);
                    var partWorkingDir = Path.GetDirectoryName(partFiles.First());
                    await FFmpeg.ExecuteAsync(ffmpegPath, arguments, partWorkingDir, onMessage, token);
                }
            }

            string GetFormatExt()
            {
                if (outputFormat == OutputFormat.MP4)
                    return ".mp4";
                if (outputFormat == OutputFormat.TS)
                    return ".ts";
                if (outputFormat == OutputFormat.M4A)
                    return ".m4a";
                if (outputFormat == OutputFormat.SRT)
                    return ".srt";
                return "";
            }

            var files = Directory.GetFiles(workDir)
                .Where(it =>
                    it.EndsWith(".mp4") ||
                    it.EndsWith(".ts") ||
                    it.EndsWith(".m4a") ||
                    it.EndsWith(".srt"))
                .OrderBy(it => it)
                .ToList();

            if (files.Count == 0)
            {
                return "";
            }

            if (files.Count == 1)
            {
                var file = files.First();
                var ext = Path.GetExtension(file);
                if (ext == GetFormatExt())
                {
                    var finishPath = Path.Combine(outputDir, $"{saveName}{ext}");
                    if (File.Exists(finishPath))
                        finishPath = Path.Combine(outputDir,
                            $"{saveName}_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}{ext}");
                    File.Move(file, finishPath);
                    return finishPath;
                }
            }

            // Multiple or format
            {
                var outputPath = Path.Combine(workDir, $"output");
                var concatTextPath = Path.Combine(workDir, $"concat.txt");
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
                if (outputFormat != OutputFormat.MP4 &&
                    outputFormat != OutputFormat.TS &&
                    outputFormat != OutputFormat.M4A)
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

                await FFmpeg.ExecuteAsync(ffmpegPath, arguments, null, onMessage, token);
                File.Delete(concatTextPath);

                var output = Directory.GetFiles(workDir)
                    .Where(it => Path.GetFileName(it).StartsWith("output"))
                    .First();

                var ext = Path.GetExtension(output);
                var finishPath = Path.Combine(outputDir, $"{saveName}{ext}");
                if (File.Exists(finishPath))
                    finishPath = Path.Combine(outputDir,
                        $"{saveName}_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}{ext}");
                logger.LogDebug("output is {0}, finishPaht is {1}", output, finishPath);
                File.Move(output, finishPath);

                foreach (var file in files)
                    File.Delete(file);
                return finishPath;
            }
        }

        public bool Support(BaseDownloadableItem item)
        {
            return item is not null && item is M3U8Item;
        }
    }
}
