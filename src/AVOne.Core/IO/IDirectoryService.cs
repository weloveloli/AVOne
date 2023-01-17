// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.IO
{
    public interface IDirectoryService
    {
        FileSystemMetadata[] GetFileSystemEntries(string path);
        List<FileSystemMetadata> GetFiles(string path);

        FileSystemMetadata GetFile(string path);

        IReadOnlyList<string> GetFilePaths(string path);

        IReadOnlyList<string> GetFilePaths(string path, bool clearCache, bool sort = false);
    }
}
