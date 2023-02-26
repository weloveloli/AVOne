// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.IO
{
    using AVOne.Constants;

    /// <summary>
    /// Helper class to create async <see cref="FileStream" />s.
    /// </summary>
    public static class FileOptionsHelper
    {
        /// <summary>
        /// Gets the default <see cref="FileStreamOptions"/> for reading files async.
        /// </summary>
        public static FileStreamOptions AsyncReadOptions => new FileStreamOptions()
        {
            Options = System.IO.FileOptions.Asynchronous
        };

        /// <summary>
        /// Gets the default <see cref="FileStreamOptions"/> for writing files async.
        /// </summary>
        public static FileStreamOptions AsyncWriteOptions => new FileStreamOptions()
        {
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = System.IO.FileOptions.Asynchronous
        };

        /// <summary>
        /// Gets the default <see cref="FileStreamOptions"/> for writing files async.
        /// </summary>
        public static FileStreamOptions SyncWriteOptions => new FileStreamOptions()
        {
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = System.IO.FileOptions.None
        };

        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A read-only <see cref="FileStream" /> on the specified path.</returns>
        public static FileStream OpenRead(string path)
            => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, AVOneConstants.FileStreamBufferSize, System.IO.FileOptions.Asynchronous);

        /// <summary>
        /// Opens an existing file for writing.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <returns>An unshared <see cref="FileStream" /> object on the specified path with Write access.</returns>
        public static FileStream OpenWrite(string path)
            => new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, AVOneConstants.FileStreamBufferSize, System.IO.FileOptions.Asynchronous);
    }
}
