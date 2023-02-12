
// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable
namespace AVOne.Resolvers
{
    using AVOne.Configuration;
    using AVOne.IO;
    using AVOne.Models.Item;

    /// <summary>
    /// These are arguments relating to the file system that are collected once and then referred to
    /// whenever needed.  Primarily for entity resolution.
    /// </summary>
    public class ItemResolveArgs
    {
        /// <summary>
        /// The _app paths.
        /// </summary>
        private readonly IApplicationPaths _appPaths;

        private BaseApplicationConfiguration _baseApplicationConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemResolveArgs" /> class.
        /// </summary>
        /// <param name="appPaths">The app paths.</param>
        /// <param name="directoryService">The directory service.</param>
        public ItemResolveArgs(IApplicationPaths appPaths, IDirectoryService directoryService)
        {
            _appPaths = appPaths;
            DirectoryService = directoryService;
        }

        // TODO remove dependencies as properties, they should be injected where it makes sense
        public IDirectoryService DirectoryService { get; }

        /// <summary>
        /// Gets or sets the file system children.
        /// </summary>
        /// <value>The file system children.</value>
        public FileSystemMetadata[] FileSystemChildren { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public Folder Parent { get; set; }

        /// <summary>
        /// Gets or sets the file info.
        /// </summary>
        /// <value>The file info.</value>
        public FileSystemMetadata FileInfo { get; set; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path => FileInfo.FullName;

        /// <summary>
        /// Gets a value indicating whether this instance is directory.
        /// </summary>
        /// <value><c>true</c> if this instance is directory; otherwise, <c>false</c>.</value>
        public bool IsDirectory => FileInfo.IsDirectory;

        /// <summary>
        /// Gets a value indicating whether this instance is vf.
        /// </summary>
        /// <value><c>true</c> if this instance is vf; otherwise, <c>false</c>.</value>
        public bool IsVf => false;

        /// <summary>
        /// Gets or sets the additional locations.
        /// </summary>
        /// <value>The additional locations.</value>
        private List<string> AdditionalLocations { get; set; }

        /// <summary>
        /// Gets the physical locations.
        /// </summary>
        /// <value>The physical locations.</value>
        public string[] PhysicalLocations
        {
            get
            {
                var paths = string.IsNullOrEmpty(Path) ? Array.Empty<string>() : new[] { Path };
                return AdditionalLocations == null ? paths : paths.Concat(AdditionalLocations).ToArray();
            }
        }

        public string CollectionType { get; set; }

        public bool HasParent<T>()
            where T : Folder
        {
            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ItemResolveArgs);
        }

        /// <summary>
        /// Adds the additional location.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c> or empty.</exception>
        public void AddAdditionalLocation(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("The path was empty or null.", nameof(path));
            }

            AdditionalLocations ??= new List<string>();
            AdditionalLocations.Add(path);
        }

        // REVIEW: @bond

        /// <summary>
        /// Gets the name of the file system entry by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>FileSystemInfo.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c> or empty.</exception>
        public FileSystemMetadata GetFileSystemEntryByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name was empty or null.", nameof(name));
            }

            return GetFileSystemEntryByPath(System.IO.Path.Combine(Path, name));
        }

        /// <summary>
        /// Gets the file system entry by path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>FileSystemInfo.</returns>
        /// <exception cref="ArgumentNullException">Throws if path is invalid.</exception>
        public FileSystemMetadata GetFileSystemEntryByPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("The path was empty or null.", nameof(path));
            }

            foreach (var file in FileSystemChildren)
            {
                if (string.Equals(file.FullName, path, StringComparison.Ordinal))
                {
                    return file;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether [contains file system entry by name] [the specified name].
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if [contains file system entry by name] [the specified name]; otherwise, <c>false</c>.</returns>
        public bool ContainsFileSystemEntryByName(string name)
        {
            return GetFileSystemEntryByName(name) != null;
        }

        public string GetCollectionType()
        {
            return CollectionType;
        }

        /// <summary>
        /// Gets the file system children that do not hit the ignore file check.
        /// </summary>
        /// <remarks>
        /// This is subject to future refactoring as it relies on a static property in BaseItem.
        /// </remarks>
        /// <returns>The file system children that are not ignored.</returns>
        public IEnumerable<FileSystemMetadata> GetActualFileSystemChildren()
        {
            return FileSystemChildren;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return Path.GetHashCode(StringComparison.Ordinal);
        }

        /// <summary>
        /// Equals the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns><c>true</c> if the arguments are the same, <c>false</c> otherwise.</returns>
        protected bool Equals(ItemResolveArgs args)
        {
            if (args != null)
            {
                if (args.Path == null && Path == null)
                {
                    return true;
                }

                return args.Path != null && BaseItem.FileSystem.AreEqual(args.Path, Path);
            }

            return false;
        }
    }
}
