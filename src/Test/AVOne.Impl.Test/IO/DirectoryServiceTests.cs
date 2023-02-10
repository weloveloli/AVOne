// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.IO.Tests
{
    using Xunit;
    using AVOne.Impl.Test;
    using AutoFixture;
    using Microsoft.Extensions.Logging;
    using Moq;
    using AVOne.Configuration;
    using AVOne.IO;
    using System.IO;

    public class DirectoryServiceTests : BaseTestCase
    {
        private IDirectoryService _directoryService;
        public DirectoryServiceTests()
        {
            fixture.Freeze<Mock<ILogger<DirectoryService>>>();
            fixture.Freeze<Mock<ILogger<ManagedFileSystem>>>();
            fixture.Freeze<Mock<IApplicationPaths>>();
            fixture.Register<ILogger<ManagedFileSystem>, IApplicationPaths, IFileSystem>((ILogger<ManagedFileSystem> logger, IApplicationPaths path) => new ManagedFileSystem(logger, path));
            fixture.Register<IFileSystem, IDirectoryService>((IFileSystem system) => new DirectoryService(system));
            _directoryService = fixture.Create<IDirectoryService>();
        }

        [Fact()]
        public void GetFileSystemEntriesTest()
        {
            var filesEntries = _directoryService.GetFiles(Path.Combine("files"));
            Assert.NotNull(filesEntries);
            Assert.NotEmpty(filesEntries);
        }

        [Fact()]
        public void GetFilesTest()
        {
            var files = _directoryService.GetFiles(Path.Combine("files", "movie"));
            Assert.NotNull(files);
            var tragets = files.Where(f => f.Name.StartsWith("stars-507"));
            Assert.NotNull(tragets);
            Assert.Equal(5, tragets.Count());
        }

        [Fact()]
        public void GetFileTest()
        {
            var file = _directoryService.GetFile(Path.Combine("files", "movie", "stars-507-C.nfo"));

            Assert.NotNull(file);
        }

        [Fact()]
        public void GetFilePathsTest()
        {
            var filePaths = _directoryService.GetFilePaths(Path.Combine("files", "movie"));
            Assert.NotNull(filePaths);
            var tragets = filePaths.Where(f => f.Contains("stars-507"));
            Assert.NotNull(tragets);
            Assert.Equal(5, tragets.Count());
        }
    }
}
