// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using CommandLine;
    using System.Threading.Tasks;
    using System.Threading;
    using AVOne.Providers.Download;
    using AVOne.Models.Download;

    [Verb("download", false, HelpText = "HelpTextVerbDownload")]
    internal class Download : BaseHostOptions
    {
        public override Task ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            var downloader = host.GetExports<IDownloaderProvider>().FirstOrDefault();

            var item = new M3U8Item
            {
                Url = "https://k-3325-bbg.thisiscdn.com/bcdn_token=OYliAa_Gy0v-7YrGcZLgGAHsO0rPZ3fWVph3kRANo8c&expires=1677387428&token_path=%2F73e9767b-7e7f-42fa-9dbc-6f03e237a150%2F/73e9767b-7e7f-42fa-9dbc-6f03e237a150/1280x720/video.m3u8",
                Header = new Dictionary<string, string>
                {
                    {"referer", "https://missav.com" }
                },
                Name = "cus-1468"
            };
            var opts = new DownloadOpts()
            {
                ThreadCount = 4,
            };
            return downloader!.CreateTask(item, opts, token);
        }
    }
}
