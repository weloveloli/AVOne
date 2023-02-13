// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Library.Tests
{
    using Xunit;
    using AVOne.Impl.Library;
    using AutoFixture;
    using AVOne.Configuration;
    using AVOne.Impl.IO;
    using AVOne.IO;
    using Microsoft.Extensions.Logging;
    using Moq;
    using AVOne.Library;
    using AVOne.Abstraction;
    using AVOne.Providers;
    using AVOne.Resolvers;
    using AVOne.Impl.Resolvers;
    using AVOne.Providers.Jellyfin;
    using AVOne.Models.Item;
    using Jellyfin.Data.Entities.Libraries;
    using AVOne.Enum;
    using System.Collections.Generic;
    using AVOne.Constants;

    public class LibraryManagerTests : BaseTestCase
    {
        private ILibraryManager _libraryManager;
        private readonly Mock<IFileSystem> _fileSystemMock;
        public LibraryManagerTests()
        {
            fixture.Freeze<Mock<ILogger<LibraryManager>>>();
            fixture.Freeze<Mock<ILogger<ManagedFileSystem>>>();
            fixture.Freeze<Mock<ILogger<MovieResolver>>>();
            fixture.Freeze<Mock<ILogger<BaseItem>>>();
            fixture.Freeze<Mock<IApplicationHost>>();
            var configMock = fixture.Freeze<Mock<IApplicationPaths>>();
            configMock.Setup(c => c.ProgramDataPath).Returns("/data");

            var managerMock = fixture.Freeze<Mock<IConfigurationManager>>();
            managerMock.Setup(m => m.ApplicationPaths.ProgramDataPath).Returns("/data");
            fixture.Register<INamingOptionProvider>(() => new JellyfinNamingOptionProvider());
            var mockNamingOptionProvider = fixture.Freeze<Mock<INamingOptionProvider>>();
            var mockProviderManager = fixture.Freeze<Mock<IProviderManager>>();
            mockProviderManager.Setup(p => p.GetNamingOptionProvider()).Returns(fixture.Create<INamingOptionProvider>());
            mockProviderManager.Setup(p => p.GetVideoResolverProvider()).Returns(new DefaultVideoResolverProvider());
            _fileSystemMock = fixture.Freeze<Mock<IFileSystem>>();
            _fileSystemMock.Setup(f => f.GetFileInfo(It.IsAny<string>())).Returns<string>(path => new FileSystemMetadata { FullName = path });
            fixture.Register((ILogger<MovieResolver> logger, IProviderManager manager) => new MovieResolver(logger, manager));
            fixture.Register
                ((IApplicationHost host, ILogger<LibraryManager> logger, IFileSystem filesystem, IConfigurationManager manager) => new LibraryManager(host, logger, filesystem, manager, mockProviderManager.Object));

            _libraryManager = fixture.Build<LibraryManager>().Do(s => s.AddParts(
            fixture.Create<IEnumerable<IResolverIgnoreRule>>(),
            new List<IItemResolver> { fixture.Create<MovieResolver>() }))
            .Create();
            // This is pretty terrible but unavoidable
            BaseItem.FileSystem ??= fixture.Create<IFileSystem>();
            BaseItem.Logger ??= fixture.Create<ILogger<BaseItem>>();

        }

        // create a function that test the constructor of LibraryManager
        [Fact()]
        public void LibraryManagerTest()
        {
            // arrange

            // act
            // assert
            Assert.NotNull(_libraryManager);
        }

        [Fact]
        public void FindExtras_SeparateMovieFolder_FindsCorrectExtras()
        {
            var owner = new PornMovie { Name = "Up", Path = "/movies/Up/Up.mkv" };
            var paths = new List<string>
        {
            "/movies/Up/Up.mkv",
            "/movies/Up/Up - trailer.mkv",
            "/movies/Up/Up - sample.mkv",
            "/movies/Up/Up something else.mkv",
            "/movies/Up/Up-extra.mkv"
        };

            var files = paths.Select(p => new FileSystemMetadata
            {
                FullName = p,
                IsDirectory = false
            }).ToList();
            var extras = _libraryManager.FindExtras(owner, files, new DirectoryService(_fileSystemMock.Object)).OrderBy(e => e.ExtraType).ToList();

            Assert.Equal(2, extras.Count);
            Assert.Equal(ExtraType.Unknown, extras[0].ExtraType);
            Assert.Equal(ExtraType.Sample, extras[1].ExtraType);
        }

        [Fact]
        public void FindMovieTest()
        {
            var paths = new List<string>
        {
            "/movies/Up/Up.mkv",
            "/movies/Up/Up - trailer.mkv",
            "/movies/Up/Up - sample.mkv",
            "/movies/Up/Up-extra.mkv"
        };
            var folder = new Folder { Name = "Up", Path = "/movies/Up" };
            var files = paths.Select(p => new FileSystemMetadata
            {
                FullName = p,
                IsDirectory = false,
                Name = Path.GetFileNameWithoutExtension(p),
                CreationTimeUtc = DateTime.UtcNow,
                LastWriteTimeUtc = DateTime.UtcNow,
                Exists = true,
            }).ToList();

            var filesMap = files.ToDictionary(f => f.FullName, f => f);
            _fileSystemMock.Setup(e => e.GetFileInfo(It.IsAny<string>())).Returns<string>(p => filesMap[p]);
            var items = _libraryManager.ResolvePaths(files, new DirectoryService(_fileSystemMock.Object), folder, default, CollectionType.PronMovies);
            var itemList = new List<BaseItem>(items);
            Assert.Single(items);
            Assert.IsType<PornMovie>(itemList[0]);
        }
    }
}
