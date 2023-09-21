// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Enum;
    using AVOne.Impl;
    using AVOne.Impl.Facade;
    using AVOne.Impl.Models;
    using AVOne.Models.Item;
    using AVOne.Tool.Resources;
    using CommandLine;
    using CommandLine.Text;
    using Spectre.Console;
    using Spectre.Console.Rendering;

    [Verb("scan", false, HelpText = "HelpTextVerbScan", ResourceType = typeof(Resource))]
    internal class Scan : BaseHostOptions
    {
        [Option('s', "save-metadata", Required = false, HelpText = nameof(Resource.HelpTextSaveMetadata), ResourceType = typeof(Resource))]
        public bool SaveMetadata { get; set; }

        [Option('t', "target-folder", Required = false, HelpText = nameof(Resource.HelpTextMoveToTargetFolder), ResourceType = typeof(Resource))]
        public string? TargetFolder { get; set; }

        [Option('c', "container", Required = false, HelpText = nameof(Resource.HelpTextUseContainer), ResourceType = typeof(Resource))]
        public bool UseContainer { get; set; }

        [Option('d', "dir", Group = "target", Required = false, HelpText = nameof(Resource.HelpTextScanDir), ResourceType = typeof(Resource))]
        public string? Dir { get; set; }

        [Option('p', "search-pattern", Required = false, HelpText = nameof(Resource.HelpTextScanDirSearchPattern), ResourceType = typeof(Resource))]
        public string? SearchPattern { get; set; }

        [Option('f', "file", Group = "target", Required = false, HelpText = nameof(Resource.HelpTextScanPath), ResourceType = typeof(Resource))]
        public string? FilePath { get; set; }

        [Option('n', "provider-name", Required = false, HelpText = "ProviderName")]
        public string? ProviderName { get; set; }
        [Option('i', "provider-id", Required = false, HelpText = "ProviderId")]
        public string? ProviderId { get; set; }
        [Usage(ApplicationAlias = ToolAlias)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {

                    new Example(string.Format(Resource.ScanSearchPatternExample1,"d:/movie/" ), new Scan { SaveMetadata = false, Dir ="d:/movie/",SearchPattern="fc*.mp4", FFmpegPath = null }),
                    new Example(string.Format(Resource.ScanSingleExample,"d:/jav","d:/movie/abc.mp4" ), new Scan { TargetFolder = "d:/jav", SaveMetadata = true, FilePath ="d:/movie/abc.mp4", FFmpegPath = null }),
                    new Example(string.Format(Resource.ScanSearchPatternExample,"d:/jav","d:/movie/" ), new Scan { TargetFolder = "d:/jav", SaveMetadata = true, UseContainer = true, Dir ="d:/movie/",SearchPattern="fc*.mp4", FFmpegPath = null })
                };
            }
        }

        public override Task ExecuteAsync(ApplicationAppHost host, CancellationToken token)
        {
            if (TargetFolder is not null)
            {
                if (!Directory.Exists(TargetFolder))
                {
                    throw Oops.Oh(ErrorCodes.DIR_NOT_EXIST, TargetFolder);
                }
            }
            if (Dir is not null)
            {
                return ScanFolder(host, token);
            }
            else if (FilePath is not null)
            {
                return ScanFile(host, token);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private async Task ScanFile(ApplicationAppHost host, CancellationToken token)
        {
            if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
            {
                throw Oops.Oh(ErrorCodes.PATH_NOT_EXIST, FilePath);
            }

            await AnsiConsole.Status().StartAsync(L.Text["Searching Metadata"]
                , async ctx =>
            {
                var facade = host.Resolve<IMetaDataFacade>();
                MetadataOpt? opt = null;
                if (!string.IsNullOrEmpty(ProviderId) && !string.IsNullOrEmpty(ProviderName))
                {
                    opt = new MetadataOpt()
                    {
                        ProviderId = ProviderId,
                        ProviderName = ProviderName,
                    };
                }
                var item = await facade.ResolveAsMovie(FilePath, token, opt);
                if (item is null)
                {
                    Cli.WarnLocale("Not a movie", FilePath);
                    return;
                }

                if (!item.HasMetaData)
                {
                    Cli.WarnLocale("Can't find metadata", item.Source.Path);
                    return;
                }

                if (!SaveMetadata && string.IsNullOrEmpty(TargetFolder))
                {
                    PrintMetaData(item);
                }

                var orignalName = item.Source.FileNameWithoutExtension;
                item.StatusChanged += (object? sender, StatusChangeArgs e) => ctx.Status(Markup.Escape(e.StatusMessage));
                var itemValid = false;
                if (!string.IsNullOrEmpty(TargetFolder))
                {
                    itemValid = MoveFile(item);
                }
                if (SaveMetadata && itemValid)
                {
                    ctx.Status(Markup.Escape(string.Format(L.Text["Saving metadata"], item.Source.Path)));
                    await facade.SaveMetaDataToLocal(item);
                }
            });
        }

        private async Task ScanFolder(ApplicationAppHost host, CancellationToken token)
        {
            if (string.IsNullOrEmpty(Dir) || !Directory.Exists(Dir))
            {
                throw Oops.Oh(ErrorCodes.DIR_NOT_EXIST, Dir);
            }

            await AnsiConsole.Status().StartAsync(L.Text["Searching Metadata"],
                async ctx =>
                {
                    IMetaDataFacade? facade = host.Resolve<IMetaDataFacade>();
                    var items = facade.ResolveAsMovies(dir: Dir, SearchPattern, token);
                    await foreach (var item in items)
                    {
                        if (!item.HasMetaData)
                        {
                            Cli.WarnLocale("Can't find metadata", item.Source.Path);
                            continue;
                        }
                        if (!SaveMetadata && string.IsNullOrEmpty(TargetFolder))
                        {
                            PrintMetaData(item);
                        }

                        var orignalName = item.Source.FileNameWithoutExtension;
                        item.StatusChanged += (object? sender, StatusChangeArgs e) => ctx.Status(Markup.Escape(e.StatusMessage));
                        var itemValid = false;
                        if (!string.IsNullOrEmpty(TargetFolder))
                        {
                            itemValid = MoveFile(item);
                        }
                        if (SaveMetadata && itemValid)
                        {
                            ctx.Status(Markup.Escape(string.Format(L.Text["Saving metadata"], item.Source.Path)));
                            await facade.SaveMetaDataToLocal(item);
                        }
                    }
                });
        }

        private void PrintMetaData(params MoveMetaDataItem[] items)
        {
            Cli.PrintTableEnum(items, false,
            ("Name", (MoveMetaDataItem e) => new Text(e.Name)),
            ("HasMetaData", (MoveMetaDataItem e) => new Text(e.HasMetaData.ToString())),
            ("MetaData", GenerateRenderable));
        }

        private IRenderable GenerateRenderable(MoveMetaDataItem item)
        {
            if (!item.HasMetaData)
            {
                return new Text(string.Empty);
            }
            var table = new Table();

            table.AddColumn("Name");
            table.AddColumn("Value");
            table.AddRow(new Text("Overview"), new Text(item.MovieWithMetaData.Overview ?? string.Empty));
            table.AddRow(new Text("Genres"), new Text(string.Join(",", item.MovieWithMetaData.Genres)));
            return table;
        }

        private bool MoveFile(MoveMetaDataItem item)
        {
            string folder = TargetFolder!;
            if (!item.HasMetaData || item.MovieWithMetaData is null)
            {
                Cli.WarnLocale(nameof(ErrorCodes.SKIP_METADATA_NOT_EXIST), item.Source.Path);
                return false;
            }
            PornMovie movieWithMetaData = item.MovieWithMetaData!;
            string newName = movieWithMetaData.PornMovieInfo.Id!;
            if (string.IsNullOrEmpty(newName))
            {
                Cli.WarnLocale(nameof(ErrorCodes.SKIP_METADATA_NOT_VALID), item.Source.Path);
                return false;
            }

            if (UseContainer)
            {
                folder = Path.Combine(folder, newName);
            }
            var newFileName = newName + Path.GetExtension(movieWithMetaData.Path);
            Directory.CreateDirectory(folder);

            var targetPath = Path.Join(folder, newFileName);

            if (File.Exists(targetPath))
            {
                Cli.WarnLocale(nameof(ErrorCodes.SKIP_FILE_DUE_TO_TARGET_FILE_AREADY_EXIST), movieWithMetaData.Path, targetPath);
                return false;
            }

            if (targetPath != movieWithMetaData.Path)
            {
                File.Move(movieWithMetaData.Path, targetPath);
                Cli.SuccessLocale("Moving file successfully", movieWithMetaData.Path, targetPath);
                movieWithMetaData.Path = targetPath;
            }
            if (item.Source.AdditionalParts?.Any() ?? false)
            {
                var newAdditionalParts = new List<string>();
                foreach (var part in item.Source.AdditionalParts)
                {
                    var partTargetPath = Path.Join(folder, Path.GetFileName(part));
                    if (partTargetPath != part)
                    {
                        File.Move(part, partTargetPath);
                        Cli.SuccessLocale("Moving additional part successfully", part, partTargetPath);
                    }
                    newAdditionalParts.Add(partTargetPath);
                }
                item.Source.AdditionalParts = newAdditionalParts.ToArray();
                movieWithMetaData.AdditionalParts = newAdditionalParts.ToArray();
            }

            movieWithMetaData.TargetPath = targetPath;
            movieWithMetaData.TargetName = newName;
            movieWithMetaData.Name = newName;
            return true;
        }
    }
}
