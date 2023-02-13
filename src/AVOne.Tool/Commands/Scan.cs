// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Constants;
    using AVOne.IO;
    using AVOne.Library;
    using AVOne.Models.Item;
    using AVOne.Tool.Models;
    using AVOne.Tool.Resources;
    using CommandLine;
    using ConsoleTables;

    [Verb("scan", false, new string[] { "scan" }, HelpText = "HelpTextVerbScan", ResourceType = typeof(Resource))]
    internal class Scan : BaseOptions
    {
        [Value(0, Required = true, HelpText = "HelpTextDirectory", ResourceType = typeof(Resource))]
        public string? Dir { get; set; }

        public override async Task<int> ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            if (string.IsNullOrEmpty(Dir) || !Directory.Exists(Dir))
            {
                Console.Error.WriteLine(Resource.ErrorDirectoryNotExists);
                return -1;
            }

            var fullpath = Path.GetFullPath(Dir);
            var libManager = host.Resolve<ILibraryManager>();
            var directoryService = host.Resolve<IDirectoryService>();
            var folder = new Folder { Path = fullpath };
            var files = directoryService.GetFiles(fullpath);
            var items = libManager.ResolvePaths(files, directoryService, folder, default, CollectionType.PronMovies);
            var rows = items.OfType<PornMovie>().Select(e => new BaseItemRow { Name = e.Name, Path = e.Path });

            ConsoleTable
                .From<BaseItemRow>(rows)
                .Configure(o => o.NumberAlignment = Alignment.Left)
                .Write(Format.Alternative);
            return 0;

        }
    }
}
