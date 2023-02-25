// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Download
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Enum;
    using AVOne.Common.Helper;
    using AVOne.Configuration;
    using AVOne.Models.Download;
    using AVOne.Providers.Download;
    using Furion.FriendlyException;

    public class N_m3u8DLDownloader : IDownloaderProvider
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly IStartupOptions _options;

        public N_m3u8DLDownloader(IApplicationPaths applicationPaths, IStartupOptions options)
        {
            _applicationPaths = applicationPaths;
            _options = options;
        }

        public string Name => "N_m3u8DL-RE";

        public int Order => 0;

        public Task CreateTask(BaseDownloadableItem item, DownloadOpts opts, CancellationToken token = default)
        {
            if (item is not M3U8Item m3U8Item || string.IsNullOrEmpty(item.SaveName) || string.IsNullOrEmpty(m3U8Item.Url))
            {
                throw Oops.Oh(ErrorCodes.INVALID_DOWNLOADABLE_ITEM);
            }

            var exe = GetExecutorPath();
            var saveName = opts.PreferName ?? item.SaveName!;
            var url = m3U8Item.Url!;
            var threadCount = opts.ThreadCount ?? 4;
            var retryCount = opts.RetryCount ?? 3;
            var saveDir = opts.OutputDir ?? Directory.GetCurrentDirectory();
            var tmpDir = _applicationPaths.CachePath;
            string argrument = $"--tmp-dir {tmpDir} --save-dir {saveDir} --thread-count {threadCount}";
            argrument += $" --save-name {saveName} --download-retry-count {retryCount} --del-after-done";
            if (m3U8Item.Header is not null && m3U8Item.Header.Any())
            {
                foreach (var h in m3U8Item.Header)
                {
                    argrument += $" --header \"{h.Key}:{h.Value}\"";
                }
            }
            argrument += $" \"{url}\"";

            var info = new ProcessStartInfo(exe, argrument)
            {
                RedirectStandardError = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            Process.Start(info);
            return Task.CompletedTask;
        }

        public bool Support(BaseDownloadableItem item)
        {
            return item is M3U8Item && !string.IsNullOrEmpty(GetExecutorPath()) && ExecutableHelper.IsExecutable(GetFFmpegPath());
        }

        public string? GetExecutorPath()
        {
            return ExecutableHelper.FindExecutable(Name);
        }

        public string? GetFFmpegPath()
        {
            return _options.FFmpegPath;
        }
    }
}
