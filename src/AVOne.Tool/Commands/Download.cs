// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using CommandLine;
    using System.Threading.Tasks;
    using System.Threading;
    using AVOne.Models.Download;
    using AVOne.Tool.Resources;
    using AVOne.Providers;
    using CommandLine.Text;
    using Spectre.Console;

    [Verb("download", false, HelpText = "HelpTextVerbDownload")]
    internal class Download : BaseHostOptions
    {
        [Option('d', "directory", Required = false, HelpText = nameof(Resource.HelpTextDownloadToTargetFolder), ResourceType = typeof(Resource))]
        public string? TargetFolder { get; set; }
        [Option('w', "web", Required = false, Group ="target", HelpText = nameof(Resource.HelpTextDownloadWebUrl), ResourceType = typeof(Resource))]
        public string? Web { get; set; }

        [Usage(ApplicationAlias = ToolAlias)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("Download video to d:/download from missav.", new Download { TargetFolder = "d:/download",Web = "https://missav.com/cus-1468" })
                };
            }
        }
        public override async Task ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            var providerManager = host.Resolve<IProviderManager>();
            if (!string.IsNullOrEmpty(Web))
            {
                var extractor = providerManager.GetMediaExtractorProviders(Web).FirstOrDefault();
                if (extractor is null)
                {
                    Cli.ErrorLocale("Can not find extractor for web url", Web);
                }
                var items = await extractor.ExtractAsync(Web, token);
                var count = items.Count();
                BaseDownloadableItem? downloadableItem = null;
                if (count == 0)
                {
                    Cli.ErrorLocale("Can not find any media for web url", Web);
                }
                else if (count > 1)
                {
                    // Choose a media to download
                    downloadableItem = AnsiConsole.Prompt(
                        new SelectionPrompt<BaseDownloadableItem>()
                            .Title(L.Text["Choose a media to download"])
                            .UseConverter(item => Markup.Escape(item.DisplayName))
                            .PageSize(10)
                            .AddChoices(items));
                }

                var downloadProvider = providerManager.GetDownloaderProviders(downloadableItem!).FirstOrDefault();
                if (downloadProvider is null)
                {
                    Cli.ErrorLocale("Can not download media", downloadableItem!.DisplayName);
                    return;
                }

                await downloadProvider!.CreateTask(downloadableItem!, new DownloadOpts { ThreadCount = 1, OutputDir = TargetFolder, RetryCount = 20, RetryWait = 500 });
            }
        }
    }
}
