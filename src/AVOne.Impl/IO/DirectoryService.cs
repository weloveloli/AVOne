// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.IO
{
    using System.Collections.Concurrent;
    using AVOne.IO;

    public class DirectoryService : IDirectoryService
    {
        private readonly IFileSystem _fileSystem;

        private readonly ConcurrentDictionary<string, FileSystemMetadata[]> _cache = new(StringComparer.Ordinal);

        private readonly ConcurrentDictionary<string, FileSystemMetadata> _fileCache = new(StringComparer.Ordinal);

        private readonly ConcurrentDictionary<string, List<string>> _filePathCache = new(StringComparer.Ordinal);

        public DirectoryService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public FileSystemMetadata[] GetFileSystemEntries(string path)
        {
            return _cache.GetOrAdd(path, static (p, fileSystem) => fileSystem.GetFileSystemEntries(p).ToArray(), _fileSystem);
        }

        public List<FileSystemMetadata> GetFiles(string path)
        {
            var list = new List<FileSystemMetadata>();
            var items = GetFileSystemEntries(path);
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (!item.IsDirectory)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        public FileSystemMetadata? GetFile(string path)
        {
            if (!_fileCache.TryGetValue(path, out var result))
            {
                var file = _fileSystem.GetFileInfo(path);
                if (file.Exists)
                {
                    result = file;
                    _fileCache.TryAdd(path, result);
                }
            }

            return result;
        }

        public IReadOnlyList<string> GetFilePaths(string path)
            => GetFilePaths(path, false);

        public IReadOnlyList<string> GetFilePaths(string path, bool clearCache, bool sort = false)
        {
            if (clearCache)
            {
                _filePathCache.TryRemove(path, out _);
            }

            var filePaths = _filePathCache.GetOrAdd(path, static (p, fileSystem) => fileSystem.GetFilePaths(p).ToList(), _fileSystem);

            if (sort)
            {
                filePaths.Sort();
            }

            return filePaths;
        }
    }
}
