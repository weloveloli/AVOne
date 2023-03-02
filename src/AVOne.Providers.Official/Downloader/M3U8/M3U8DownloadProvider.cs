// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.M3U8
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
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
    using AVOne.Providers.Download;
    using AVOne.Providers.Official.Downloader.M3U8.Parser;
    using AVOne.Providers.Official.Downloader.M3U8.Utils;
    using Furion.FriendlyException;
    using Furion.Localization;
    using Jint.Parser;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;

    public class M3U8DownloadProvider : IDownloaderProvider
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly IStartupOptions _options;
        private readonly HttpClient _client;

        public M3U8DownloadProvider(IApplicationPaths applicationPaths, IStartupOptions options, IHttpClientFactory httpClientFactory)
        {
            _applicationPaths = applicationPaths;
            _options = options;
            _client = httpClientFactory.CreateClient(AVOneConstants.Download);
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
            MediaPlaylist mediaPlaylist;
            if (File.Exists(Path.Combine(workingDir, playMetaDataFile)))
            {
                mediaPlaylist = JsonSerializer.Deserialize<MediaPlaylist>(File.ReadAllText(Path.Combine(workingDir, playMetaDataFile)));
            }
            else
            {
                mediaPlaylist = await GetMediaPlaylist(workingDir, url, saveName, m3U8Item, token);
                File.WriteAllText(Path.Combine(workingDir, playMetaDataFile), JsonSerializer.Serialize(mediaPlaylist));
            }

            if (mediaPlaylist is null)
            {
                throw Oops.Oh(ErrorCodes.INVALID_DOWNLOADABLE_ITEM);
            }

            DownloadParts(workingDir, mediaPlaylist, opts, m3U8Item, threadCount, token);

            MergeAsync(workingDir, saveDir, saveName, OutputFormat.MP4, token: token).Wait();

            Directory.Delete(workingDir, true);
        }

        private void DownloadParts(string workingDir, MediaPlaylist mediaPlaylist, DownloadOpts opts, M3U8Item m3U8Item, int threadCount, CancellationToken token)
        {

            var finished = 0;
            var total = mediaPlaylist.Parts.Select(e => e.Segments.Count).Sum();
            foreach (var (part, index) in mediaPlaylist.Parts.Select((e, i) => (e, i)))
            {
                var partDir = Path.Combine(workingDir, $"Part_{index}");
                Directory.CreateDirectory(partDir);
                var list = mediaPlaylist.Parts[index].Segments;
                var numbers = Enumerable.Range(0, list.Count).ToList();
                Parallel.ForEach(numbers, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, index =>
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

                        foreach (var header in m3U8Item.Header)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }

                        var rsp = _client.Send(request);
                        rsp.EnsureSuccessStatusCode();
                        var stream = File.OpenWrite(tsTmpPath);
                        rsp.Content.CopyTo(stream, null, token);
                        stream.Flush();
                        stream.Close();
                        File.Move(tsTmpPath, tsPath);
                    }
                    var done = Interlocked.Increment(ref finished);
                    opts.OnStatusChanged(new DownloadStatusArgs { Status = $"{done}/{total}" });
                });
            }
        }

        private async Task<MediaPlaylist> GetMediaPlaylist(string workingDir, string url, string saveName, M3U8Item m3U8Item, CancellationToken token)
        {
            var md5 = url.GetMD5();
            var playListName = $"{saveName}-{md5}.m3u8";
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
            };

            foreach (var header in m3U8Item.Header)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var rsp = await _client.SendAsync(request, token);
            rsp.EnsureSuccessStatusCode();
            var m3u8 = await rsp.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(m3u8))
            {
                throw Oops.Oh(ErrorCodes.INVALID_DOWNLOADABLE_ITEM);
            }
            File.WriteAllText(Path.Combine(workingDir, playListName), m3u8);
            var parser = new MediaPlaylistParser();
            return parser.Parse(m3u8, url);
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
            bool genpts = false, bool igndts = false, bool ignidx = false,
            bool clearTempFile = false, Action<string>? onMessage = null,
            CancellationToken token = default)
        {
            var ffmpegPath = _options.FFmpegPath ?? "ffmepge";
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

                    switch (outputFormat)
                    {
                        case OutputFormat.MP4:
                            arguments += $@"-map 0:v? -map 0:a? -map 0:s? -c copy -y -bsf:a aac_adtstoasc ""{partOutputPath}.mp4""";
                            break;
                        case OutputFormat.TS:
                            arguments += $@"-map 0 -c copy -y -f mpegts -bsf:v h264_mp4toannexb ""{partOutputPath}.ts""";
                            break;
                        case OutputFormat.M4A:
                            arguments += $@"-map 0:a -c copy -y ""{partOutputPath}.m4a""";
                            break;
                        case OutputFormat.SRT:
                            arguments += $@"-map 0 -y ""{partOutputPath}.srt""";
                            break;
                        default:
                            throw new Exception("OutputFormat not match.");
                    }
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
                var finishPath = Path.Combine(outputPath, $"{saveName}{ext}");
                if (File.Exists(finishPath))
                    finishPath = Path.Combine(outputPath,
                        $"{saveName}_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}{ext}");
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
