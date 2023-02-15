// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLine;
    using ConsoleTables;
    using Furion.DatabaseAccessor;
    using MediaBrowser.Controller.Entities;
    using MediaBrowser.Controller.Entities.Movies;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Controller.Providers;
    using MediaBrowser.Model.Entities;

    [Verb("scan", false, new string[] { "scan" }, HelpText = "扫描")]
    internal class Scan : BaseOptions
    {
        [Value(0, Required = true, HelpText = "HelpTextDirectory")]
        public string? Dir { get; set; }

        internal override Task<int> ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            return Task.Run(() => ScanFolder(host), token);
        }

        private int ScanFolder(ConsoleAppHost host)
        {
            if (string.IsNullOrEmpty(Dir) || !Directory.Exists(Dir))
            {
                return -1;
            }

            var fullpath = Path.GetFullPath(Dir);
            var libManager = host.Resolve<ILibraryManager>();
            var directoryService = host.Resolve<IDirectoryService>();
            var folder = new Folder { Path = fullpath };
            var files = directoryService.GetFiles(fullpath);
            var items = libManager.ResolvePaths(files, directoryService, folder, default, CollectionType.Movies);

            var table = new ConsoleTable("Name", "Path");
            table.Configure(o => o.NumberAlignment = Alignment.Left);
            foreach (var item in items)
            {
                table.AddRow(item.Name, item.Path);
            }
            table.Write(Format.Minimal);
            Console.WriteLine();
            return 0;
        }
    }
}
