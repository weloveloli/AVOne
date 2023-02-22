﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Enum;
    using AVOne.Models.Item;
    using AVOne.Tool.Facade;
    using AVOne.Tool.Models;
    using AVOne.Tool.Resources;
    using CommandLine;
    using Spectre.Console;
    using Spectre.Console.Rendering;

    [Verb("scan", false, HelpText = "HelpTextVerbScan", ResourceType = typeof(Resource))]
    internal class Scan : BaseHostOptions
    {
        [Option('s', "save-metadata", Required = false, HelpText = nameof(Resource.HelpTextSaveMetadata), ResourceType = typeof(Resource))]
        public bool SaveMetadata { get; set; }

        [Option('t', "target-folder", Required = false, HelpText = nameof(Resource.HelpTextMoveToTargetFolder), ResourceType = typeof(Resource))]
        public string? TargetFolder { get; set; }

        [Option('c', "use-container", Required = false, HelpText = nameof(Resource.HelpTextUseContainer), ResourceType = typeof(Resource))]
        public bool UseContainer { get; set; }

        [Option('d', "dir", Group = "target", Required = false, HelpText = nameof(Resource.HelpTextScanDir), ResourceType = typeof(Resource))]
        public string? Dir { get; set; }

        [Option('f', "file", Group = "target", Required = false, HelpText = nameof(Resource.HelpTextScanPath), ResourceType = typeof(Resource))]
        public string? FilePath { get; set; }

        public override Task ExecuteAsync(ConsoleAppHost host, CancellationToken token)
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

        private async Task ScanFile(ConsoleAppHost host, CancellationToken token)
        {
            if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
            {
                throw Oops.Oh(ErrorCodes.PATH_NOT_EXIST, FilePath);
            }

            await AnsiConsole.Status().StartAsync(L.Text["Searching Metadata"]
                , async ctx =>
            {
                var facade = host.Resolve<IMetaDataFacade>();
                MoveMetaDataItem? item = await facade.ResolveAsMovie(FilePath, token);

                if (!SaveMetadata && string.IsNullOrEmpty(TargetFolder))
                {
                    PrintMetaData(item);
                }
                var orignalName = item.Movie.FileNameWithoutExtension;
                if (!string.IsNullOrEmpty(TargetFolder))
                {

                    ctx.Status(string.Format(L.Text["Moving file"], orignalName));
                    MoveFile(item);
                    Cli.SuccessLocale("Moving file successfully", orignalName, item.Result.TargetPath);
                }
                if (SaveMetadata)
                {
                    ctx.Status(string.Format(L.Text["Saving metadata"], orignalName));
                    facade.SaveMetaDataToLocal(item);
                }
            });
        }

        private async Task ScanFolder(ConsoleAppHost host, CancellationToken token)
        {
            if (string.IsNullOrEmpty(Dir) || !Directory.Exists(Dir))
            {
                throw Oops.Oh(ErrorCodes.DIR_NOT_EXIST, Dir);
            }

            await AnsiConsole.Status().StartAsync(L.Text["Searching Metadata"],
                async ctx =>
                {
                    IMetaDataFacade? facade = host.Resolve<IMetaDataFacade>();
                    var items = await facade.ResolveAsMovies(dir: Dir, token);
                    if (!SaveMetadata && string.IsNullOrEmpty(TargetFolder))
                    {
                        PrintMetaData(items.ToArray());
                    }
                });
        }

        private void PrintMetaData(params MoveMetaDataItem[] items)
        {
            Cli.PrintTableEnum(items, true,
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
            table.AddRow(new Text("Overview"), new Text(item.Result.Overview));
            table.AddRow(new Text("Genres"), new Text(string.Join(",", item.Result.Genres)));
            return table;
        }

        private void MoveFile(MoveMetaDataItem item)
        {
            string folder = TargetFolder!;
            if (!item.HasMetaData || item.Result is null)
            {
                Cli.WarnLocale(nameof(ErrorCodes.SKIP_METADATA_NOT_EXIST), item.Movie.Path);
                return;
            }
            PornMovie movie = item.Result!;
            string newName = movie.PornMovieInfo.Id!;
            if (UseContainer)
            {
                folder = Path.Combine(folder, item.Result.PornMovieInfo.Id);
            }
            var newFileName = newName + Path.GetExtension(movie.Path);
            Directory.CreateDirectory(folder);

            var targetPath = Path.Join(folder, newFileName);

            if (File.Exists(targetPath))
            {
                Cli.WarnLocale(nameof(ErrorCodes.SKIP_FILE_DUE_TO_TARGET_FILE_AREADY_EXIST), movie.Path, targetPath);
                return;
            }

            if (targetPath != movie.Path)
            {
                File.Move(movie.Path, targetPath);
            }

            movie.TargetPath = targetPath;
            movie.TargetName = newName;

        }
    }
}
