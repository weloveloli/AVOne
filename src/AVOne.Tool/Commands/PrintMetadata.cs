// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Commands
{
    using AVOne.Constants;
    using AVOne.Tool.Resources;
    using CommandLine;
    using CommandLine.Text;

    [Verb("printmetadata", false, new string[] { "pm" }, HelpText = "HelpTextVerbPrintMetadata", ResourceType = typeof(Resource))]
    internal class PrintMetadata : BaseHostOptions
    {
        public bool Debug { get; set; }

        public bool Quiet { get; set; }

        /// <inheritdoc />
        [Option("type", Required = false, HelpText = "HelpTextCollectionType",
            ResourceType = typeof(Resource))]
        public string? Type { get; set; } = CollectionType.PornMovies;

        [Value(0, Required = true, HelpText = "HelpTextFileName", ResourceType = typeof(Resource))]
        public string? FileName { get; set; }

        [Usage(ApplicationAlias = "AVOneTool")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example(Resource.ExamplesNormalScenario, new PrintMetadata { FileName = "MUM-120.mp4" });
            }
        }

        public override async Task<int> ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            //var providerManager = host.Resolve<IProviderManager>();
            //var libraryManager = host.Resolve<ILibraryManager>();
            //var fileSystem = host.Resolve<IFileSystem>();
            //var directoryService = host.Resolve<IDirectoryService>();
            //if (!Path.Exists(FileName))
            //{
            //    Console.Error.WriteLine(Resource.ErrorPathNotExists, FileName);
            //    return 1;
            //}
            //var dir = Path.GetDirectoryName(FileName);
            //var parent = new Folder
            //{
            //    Name = dir,
            //    Path = dir
            //};
            //var item = libraryManager.ResolvePath(fileSystem.GetFileInfo(FileName), parent, directoryService, Type);

            //if (item is null || item is not PornMovie pornMovie)
            //{
            //    Console.Error.WriteLine(Resource.InvalidMoviePath, FileName);
            //    return 1;
            //}
            //var providers = providerManager.GetMetadataProviders<PornMovie>(item);
            //var localProviders = providers.OfType<ILocalMetadataProvider<PornMovie>>();
            //var remoteProviders = providers.OfType<IRemoteMetadataProvider<PornMovie, PornMovieInfo>>();
            //var info = pornMovie.PornMovieInfo;
            //var tasks = new List<Task<ConsoleTable?>>();
            //if (localProviders.Any())
            //{
            //    foreach (var provider in localProviders)
            //    {
            //        tasks.Add(Task.Run(async () =>
            //        {
            //            var metadata = await provider.GetMetadata(new ItemInfo(pornMovie), directoryService, CancellationToken.None);

            //            return metadata.HasMetadata ? ConsoleTableHelper.ToTable(metadata.Item, provider) : null;
            //        }));

            //    }
            //}

            //if (remoteProviders.Any())
            //{
            //    foreach (var provider in remoteProviders)
            //    {
            //        tasks.Add(Task.Run(async () =>
            //        {
            //            var metadata = await provider.GetMetadata(info, CancellationToken.None);
            //            return metadata.HasMetadata ? ConsoleTableHelper.ToTable(metadata.Item, provider) : null;
            //        }));

            //    }
            //}

            //var tables = await Task.WhenAll(tasks);

            //foreach (var table in tables)
            //{
            //    if (table is not null)
            //    {
            //        table.Write(Format.Minimal);
            //        Console.WriteLine();
            //    }
            //}
            return 0;
        }
    }
}
