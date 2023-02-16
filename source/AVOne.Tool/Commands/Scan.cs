// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Tool.Resources;
    using CommandLine;
    using MediaBrowser.Controller.Entities;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Controller.Providers;
    using MediaBrowser.Model.Entities;

    [Verb("scan", false, new string[] { "scan" }, HelpText = "HelpTextVerbScan", ResourceType = typeof(Resource))]
    internal class Scan : BaseOptions
    {
        [Value(0, Required = true, HelpText = "HelpTextDirectory")]
        public string? Dir { get; set; }

        internal override Task ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            return Task.Run(() => ScanFolder(host), token);
        }

        private void ScanFolder(ConsoleAppHost host)
        {
            if (string.IsNullOrEmpty(Dir) || !Directory.Exists(Dir))
            {
                return;
            }

            var fullpath = Path.GetFullPath(Dir);
            var libManager = host.Resolve<ILibraryManager>();
            var directoryService = host.Resolve<IDirectoryService>();
            var folder = new Folder { Path = fullpath };
            var files = directoryService.GetFiles(fullpath);
            var items = libManager.ResolvePaths(files, directoryService, folder, default, CollectionType.Movies);

            Cli.PrintTable(items, true, "Name", "Path");
            Console.WriteLine();
            return;
        }
    }
}
