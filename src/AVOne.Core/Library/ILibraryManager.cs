// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable
namespace AVOne.Library
{
    using AVOne.Configuration;
    using AVOne.IO;
    using AVOne.Models.Item;
    using AVOne.Resolvers;

    public interface ILibraryManager
    {
        /// <summary>
        /// Resolves the path.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="directoryService">An instance of <see cref="IDirectoryService"/>.</param>
        /// <returns>BaseItem.</returns>
        BaseItem ResolvePath(
            FileSystemMetadata fileInfo,
            Folder parent = null,
            IDirectoryService directoryService = null,
            string collectionType = null);

        /// <summary>
        /// Resolves a set of files into a list of BaseItem.
        /// </summary>
        /// <param name="files">The list of tiles.</param>
        /// <param name="directoryService">Instance of the <see cref="IDirectoryService"/> interface.</param>
        /// <param name="parent">The parent folder.</param>
        /// <param name="libraryOptions">The library options.</param>
        /// <param name="collectionType">The collection type.</param>
        /// <returns>The items resolved from the paths.</returns>
        IEnumerable<BaseItem> ResolvePaths(
            IEnumerable<FileSystemMetadata> files,
            IDirectoryService directoryService,
            Folder parent,
            string collectionType = null);

        /// <summary>
        /// Gets a Person.
        /// </summary>
        /// <param name="name">The name of the person.</param>
        /// <returns>Task{Person}.</returns>
        Person GetPerson(string name);

        /// <summary>
        /// Adds the parts.
        /// </summary>
        /// <param name="resolvers">The resolvers.</param>
        public void AddParts(
                IEnumerable<IResolverIgnoreRule> rules,
                IEnumerable<IItemResolver> resolvers);

        /// <summary>
        /// Gets the new item identifier.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="type">The type.</param>
        /// <returns>Guid.</returns>
        Guid GetNewItemId(string key, Type type);

        /// <summary>
        /// Finds the extras.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="fileSystemChildren">The file system children.</param>
        /// <param name="directoryService">An instance of <see cref="IDirectoryService"/>.</param>
        /// <returns>IEnumerable&lt;BaseItem&gt;.</returns>
        IEnumerable<BaseItem> FindExtras(BaseItem owner, IReadOnlyList<FileSystemMetadata> fileSystemChildren, IDirectoryService directoryService);
    }
}
