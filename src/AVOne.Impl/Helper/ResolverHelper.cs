﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Helper
{
    using AVOne.IO;
    using AVOne.Library;
    using AVOne.Models.Item;
    using AVOne.Resolvers;

    /// <summary>
    /// Class ResolverHelper.
    /// </summary>
    public static class ResolverHelper
    {
        /// <summary>
        /// Sets the initial item values.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="libraryManager">The library manager.</param>
        /// <param name="directoryService">The directory service.</param>
        /// <exception cref="ArgumentException">Item must have a path.</exception>
        public static void SetInitialItemValues(BaseItem item, Folder? parent, ILibraryManager libraryManager, IDirectoryService directoryService)
        {
            // This version of the below method has no ItemResolveArgs, so we have to require the path already being set
            if (string.IsNullOrEmpty(item.Path))
            {
                throw new ArgumentException("Item must have a Path");
            }
            // If the resolver didn't specify this
            if (parent != null)
            {
                item.SetParent(parent);
            }

            item.Id = libraryManager.GetNewItemId(item.Path, item.GetType());

            // Make sure DateCreated and DateModified have values
            var fileInfo = directoryService.GetFile(item.Path);
            if (fileInfo == null)
            {
                throw new FileNotFoundException("Can't find item path.", item.Path);
            }

            SetDateCreated(item, fileInfo);

            EnsureName(item, fileInfo);
        }

        /// <summary>
        /// Sets the initial item values.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="args">The args.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="libraryManager">The library manager.</param>
        public static void SetInitialItemValues(BaseItem item, ItemResolveArgs args, IFileSystem fileSystem, ILibraryManager libraryManager)
        {
            // If the resolver didn't specify this
            if (string.IsNullOrEmpty(item.Path))
            {
                item.Path = args.Path;
            }

            // If the resolver didn't specify this
            if (args.Parent != null)
            {
                item.SetParent(args.Parent);
            }

            item.Id = libraryManager.GetNewItemId(item.Path, item.GetType());

            // Make sure the item has a name
            EnsureName(item, args.FileInfo);

            // Make sure DateCreated and DateModified have values
            EnsureDates(fileSystem, item, args);
        }

        /// <summary>
        /// Ensures the name.
        /// </summary>
        private static void EnsureName(BaseItem item, FileSystemMetadata fileInfo)
        {
            // If the subclass didn't supply a name, add it here
            if (string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Path))
            {
                item.Name = fileInfo.IsDirectory ? fileInfo.Name : Path.GetFileNameWithoutExtension(fileInfo.Name);
            }
        }

        /// <summary>
        /// Ensures DateCreated and DateModified have values.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="item">The item.</param>
        /// <param name="args">The args.</param>
        private static void EnsureDates(IFileSystem fileSystem, BaseItem item, ItemResolveArgs args)
        {
            // See if a different path came out of the resolver than what went in
            if (!fileSystem.AreEqual(args.Path, item.Path))
            {
                var childData = args.IsDirectory ? args.GetFileSystemEntryByPath(item.Path) : null;

                if (childData != null)
                {
                    SetDateCreated(item, childData);
                }
                else
                {
                    var fileData = fileSystem.GetFileSystemInfo(item.Path);

                    if (fileData.Exists)
                    {
                        SetDateCreated(item, fileData);
                    }
                }
            }
            else
            {
                SetDateCreated(item, args.FileInfo);
            }
        }

        private static void SetDateCreated(BaseItem item, FileSystemMetadata? info)
        {

            // directoryService.getFile may return null
            if (info != null)
            {
                var dateCreated = info.CreationTimeUtc;

                if (dateCreated.Equals(DateTime.MinValue))
                {
                    dateCreated = DateTime.UtcNow;
                }

                item.DateCreated = dateCreated;
            }
        }
    }
}
