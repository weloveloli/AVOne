// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Facade
{
    using AVOne.Configuration;
    using AVOne.IO;
    using AVOne.Models.Systems;
    using Microsoft.Extensions.Logging;

    public class SystemService : ISystemService
    {
        private readonly ILogger<SystemService> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IApplicationPaths _appPaths;

        public SystemService(ILogger<SystemService> logger, IFileSystem fileSystem, IApplicationPaths applicationPaths)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _appPaths = applicationPaths;
        }
        public LogFile[] GetServerLogs()
        {
            IEnumerable<FileSystemMetadata> files;

            try
            {
                files = _fileSystem.GetFiles(_appPaths.LogDirectoryPath, new[] { ".txt", ".log" }, true, false);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error getting logs");
                files = Enumerable.Empty<FileSystemMetadata>();
            }

            var result = files.Select(i => new LogFile
            {
                DateCreated = _fileSystem.GetCreationTimeUtc(i),
                DateModified = _fileSystem.GetLastWriteTimeUtc(i),
                Name = i.Name,
                Size = i.Length
            })
                .OrderByDescending(i => i.DateModified)
                .ThenByDescending(i => i.DateCreated)
                .ThenBy(i => i.Name)
                .ToArray();

            return result;
        }
    }
}
