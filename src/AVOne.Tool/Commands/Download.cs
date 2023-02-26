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

    [Verb("download", false, HelpText = nameof(Resource.HelpTextVerbDownload), ResourceType = typeof(Resource))]
    internal class Download : BaseHostOptions
    {
        [Option('o', "output", Required = false, HelpText = nameof(Resource.HelpTextOptionSaveDir), ResourceType = typeof(Resource))]
        public string? TargetFolder { get; set; }
        [Option('w', "web-url", Required = false, Group = "target", HelpText = nameof(Resource.HelpTextDownloadWebUrl), ResourceType = typeof(Resource))]
        public string? Web { get; set; }
        [Option('t',"thread-count", Required = false, Default = 4, HelpText = nameof(Resource.HelpTextOptionThreadCount), ResourceType = typeof(Resource))]
        public int? ThreadCount { get; set; }
        [Option('n', "save-name", Required = false, HelpText = nameof(Resource.HelpTextOptionPreferName), ResourceType = typeof(Resource))]
        public string? PreferName { get; set; }

        [Option('r', "retry-count", Required = false, HelpText = nameof(Resource.HelpTextOptionRetryCount), ResourceType = typeof(Resource))]
        public int? RetryCount { get; set; }

        [Usage(ApplicationAlias = ToolAlias)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example(string.Format(Resource.DownloadVideoExample,"d:/download","missav" ), new Download { TargetFolder = "d:/download", Web = "https://missav.com/cus-1468" })
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
                    throw Oops.Oh("Can not find extractor for web url", Web);
                }
                var items = await extractor!.ExtractAsync(Web, token);
                var count = items.Count();
                BaseDownloadableItem? downloadableItem = null;
                if (count == 0)
                {
                    throw Oops.Oh("Can not find any media for web url", Web);
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
                    throw Oops.Oh("Can not download media", downloadableItem!.DisplayName);
                }

                await downloadProvider!.CreateTask(downloadableItem!, new DownloadOpts { ThreadCount = ThreadCount, OutputDir = TargetFolder, RetryCount = RetryCount, RetryWait = 500, PreferName = PreferName });
            }
        }
    }
}
