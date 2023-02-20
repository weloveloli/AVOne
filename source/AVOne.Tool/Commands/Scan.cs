// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Impl.Facade;
    using AVOne.Impl.Models;
    using AVOne.Tool.Resources;
    using CommandLine;
    using Spectre.Console;
    using Spectre.Console.Rendering;

    [Verb("scan", false, HelpText = "HelpTextVerbScan", ResourceType = typeof(Resource))]
    internal class Scan : BaseHostOptions
    {
        [Option('m', "metadata", Required = false, HelpText = nameof(Resource.HelpTextShowInstalledPlugins), ResourceType = typeof(Resource))]
        public bool Metadata { get; set; }

        [Value(0, Required = true, HelpText = "HelpTextDirectory")]
        public string? Dir { get; set; }

        internal override Task ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            return ScanFolder(host, token);
        }

        private async Task ScanFolder(ConsoleAppHost host, CancellationToken token)
        {
            if (string.IsNullOrEmpty(Dir) || !Directory.Exists(Dir))
            {
                return;
            }
            var facade = host.Resolve<IMetaDataFacade>();
            var items = await facade.ResolveAsMovies(dir: Dir ,token);
            items = items.ToList();
            Cli.PrintTableEnum(items, true,
                ("Name", (MoveMetaDataItem e) => new Text(e.Name)),
                ("HasMetaData", (MoveMetaDataItem e) => new Text(e.HasMetaData.ToString())),
                ("MetaData", GenerateRenderable)
                );
        }

        public IRenderable GenerateRenderable(MoveMetaDataItem item)
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
            var image = item.Result.ImageInfos
                .Where(e => e.Type == MediaBrowser.Model.Entities.ImageType.Primary)
                .FirstOrDefault();
            Renderable imageRender = File.Exists(image.Path) ? new CanvasImage(image.Path) : new Text(image?.Path ?? string.Empty);
            table.AddRow(new Text("Image"), imageRender);
            return table;
        }
    }
}

