// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.IO
{
    public interface IDirectoryService
    {
        FileSystemMetadata[] GetFileSystemEntries(string path);
        List<FileSystemMetadata> GetFiles(string path, string? seachOption = null);

        FileSystemMetadata GetFile(string path);

        IReadOnlyList<string> GetFilePaths(string path);

        IReadOnlyList<string> GetFilePaths(string path, bool clearCache, bool sort = false);
    }
}
