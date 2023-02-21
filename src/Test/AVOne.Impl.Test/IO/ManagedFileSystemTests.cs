// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.IO.Tests
{
    using System.IO;
    using AutoFixture;
    using AVOne.Configuration;
    using AVOne.IO;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class ManagedFileSystemTests : BaseTestCase
    {
        private IFileSystem _fileSystem;
        public ManagedFileSystemTests()
        {
            fixture.Freeze<Mock<ILogger<DirectoryService>>>();
            fixture.Freeze<Mock<ILogger<ManagedFileSystem>>>();
            fixture.Freeze<Mock<IApplicationPaths>>();
            fixture.Register<ILogger<ManagedFileSystem>, IApplicationPaths, IFileSystem>((ILogger<ManagedFileSystem> logger, IApplicationPaths path) => new ManagedFileSystem(logger, path));
            _fileSystem = fixture.Create<IFileSystem>();
        }
        [Fact()]
        public void GetFilesTest()
        {
            var files = _fileSystem.GetFiles(Path.Combine("files"));
            Assert.NotNull(files);
            Assert.NotEmpty(files);
            var tragets = files.Where(f => f.Name.StartsWith("stars-507"));
            Assert.Empty(tragets);
        }

        [Fact()]
        public void GetFilesTest1()
        {
            var files = _fileSystem.GetFiles(Path.Combine("files"), true);
            Assert.NotNull(files);
            Assert.NotEmpty(files);
            var tragets = files.Where(f => f.Name.StartsWith("stars-507"));
            Assert.NotNull(tragets);
            Assert.Equal(5, tragets.Count());
        }
    }
}

