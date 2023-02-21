// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Common.Enum;
    using AVOne.Tool.Resources;
    using CommandLine;
    using Furion.FriendlyException;

    [Verb("scan", false, new string[] { "scan" }, HelpText = "HelpTextVerbScan", ResourceType = typeof(Resource))]
    internal class Scan : BaseHostOptions
    {
        [Value(0, Required = true, HelpText = "HelpTextDirectory", ResourceType = typeof(Resource))]
        public string? Dir { get; set; }

        public override async Task<int> ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            throw Oops.Oh(ErrorCodes.ProviderNotAvailable, "Hello");
            //if (string.IsNullOrEmpty(Dir) || !Directory.Exists(Dir))
            //{
            //    Console.Error.WriteLine(Resource.ErrorDirectoryNotExists);
            //    return -1;
            //}

            //var fullpath = Path.GetFullPath(Dir);
            //var libManager = host.Resolve<ILibraryManager>();
            //var directoryService = host.Resolve<IDirectoryService>();
            //var folder = new Folder { Path = fullpath };
            //var files = directoryService.GetFiles(fullpath);
            //var items = libManager.ResolvePaths(files, directoryService, folder, default, CollectionType.PornMovies);
            //var rows = items.OfType<PornMovie>().Select(e => new BaseItemRow { Name = e.Name, Path = e.Path });

            //ConsoleTable
            //    .From<BaseItemRow>(rows)
            //    .Configure(o => o.NumberAlignment = Alignment.Left)
            //    .Write(Format.Minimal);
            return 0;

        }
    }
}
