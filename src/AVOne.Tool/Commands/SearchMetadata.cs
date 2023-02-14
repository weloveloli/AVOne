// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Commands
{
    using System;
    using AVOne.Configuration;
    using AVOne.Constants;
    using AVOne.IO;
    using AVOne.Library;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Models.Result;
    using AVOne.Providers;
    using AVOne.Tool.Extensions;
    using AVOne.Tool.Resources;
    using CommandLine;
    using CommandLine.Text;
    using ConsoleTables;

    [Verb("searchmetadata", false, new string[] { "sm" }, HelpText = "HelpTextVerbSearchMetadata", ResourceType = typeof(Resource))]
    internal class SearchMetadata : BaseOptions
    {
        public bool Debug { get; set; }

        public bool Quiet { get; set; }

        /// <inheritdoc />
        [Option("type", Required = false, HelpText = "HelpTextCollectionType",
            ResourceType = typeof(Resource))]
        public string? Type { get; set; } = CollectionType.PronMovies;

        [Value(0, Required = true, HelpText = "HelpTextFileName", ResourceType = typeof(Resource))]
        public string? FileName { get; set; }

        [Usage(ApplicationAlias = "AVOneTool")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example(Resource.ExamplesNormalScenario, new SearchMetadata { FileName = "MUM-120.mp4" });
            }
        }

        public override async Task<int> ExecuteAsync(ConsoleAppHost host, CancellationToken token)
        {
            var providerManager = host.Resolve<IProviderManager>();
            var libraryManager = host.Resolve<ILibraryManager>();
            var fileSystem = host.Resolve<IFileSystem>();
            var directoryService = host.Resolve<IDirectoryService>();
            if (!Path.Exists(FileName))
            {
                Console.Error.WriteLine(Resource.ErrorPathNotExists, FileName);
                return 1;
            }
            var dir = Path.GetDirectoryName(FileName);
            var parent = new Folder
            {
                Name = dir,
                Path = dir
            };
            var item = libraryManager.ResolvePath(fileSystem.GetFileInfo(FileName), parent, directoryService, Type);

            if (item is null || item is not PornMovie pornMovie)
            {
                Console.Error.WriteLine(Resource.InvalidMoviePath, FileName);
                return 1;
            }
            var providers = providerManager.GetMetadataProviders<PornMovie>(item);
            var localProviders = providers.OfType<ILocalMetadataProvider<PornMovie>>();
            var remoteProviders = providers.OfType<IRemoteMetadataProvider<PornMovie, PornMovieInfo>>();
            var info = pornMovie.PornMovieInfo;
            var results = new List<MetadataResult<PornMovie>>();
            var tasks = new List<Task<List<NameValue>?>>();
            if (localProviders.Any())
            {
                foreach (var localProvider in localProviders)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var metadata = await localProvider.GetMetadata(new ItemInfo(pornMovie), directoryService, CancellationToken.None);
                        return metadata.NameValues(localProvider);
                    }));

                }
            }
            if (remoteProviders.Any())
            {
                foreach (var remoteMetadataProvider in remoteProviders)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var metadata = await remoteMetadataProvider.GetMetadata(info, CancellationToken.None);
                        return metadata.NameValues(remoteMetadataProvider);
                    }));

                }
            }
            var metadatas = await Task.WhenAll(tasks);

            var tableRows = metadatas.Where(e => e is not null);

            foreach (var tableRow in tableRows)
            {
                ConsoleTable.From<NameValue>(tableRow)
                .Configure(o => o.NumberAlignment = Alignment.Left)
                .Write(Format.Default);
                Console.WriteLine();
            }
            return 0;
        }
    }
}
