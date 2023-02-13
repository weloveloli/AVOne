// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable

namespace AVOne.Impl.Library
{
    using System;
    using System.Collections.Generic;
    using AVOne.Abstraction;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Extensions;
    using AVOne.Impl.Helper;
    using AVOne.Impl.IO;
    using AVOne.Impl.Resolvers;
    using AVOne.IO;
    using AVOne.Library;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using AVOne.Resolvers;
    using Microsoft.Extensions.Logging;

    public class LibraryManager : ILibraryManager
    {

        private readonly IConfigurationManager _configurationManager;
        private readonly IProviderManager _providerManager;
        private readonly IFileSystem _fileSystem;
        private readonly IApplicationHost _appHost;
        private readonly ILogger<LibraryManager> _logger;
        private INamingOptions _namingOptions => _providerManager.GetNamingOptionProvider().GetNamingOption();
        private ExtraResolver _extraResolver;

        public LibraryManager(
            IApplicationHost appHost,
            ILogger<LibraryManager> logger, IFileSystem fileSystem, IConfigurationManager configurationManager, IProviderManager providerManager)
        {
            _appHost = appHost;
            _logger = logger;
            _fileSystem = fileSystem;
            _configurationManager = configurationManager;
            _providerManager = providerManager;
            _extraResolver = new ExtraResolver(logger, _providerManager);
        }
        /// <summary>
        /// Gets or sets the list of currently registered entity resolvers.
        /// </summary>
        /// <value>The entity resolvers enumerable.</value>
        private IItemResolver[] ItemResolvers { get; set; } = Array.Empty<IItemResolver>();

        private IMultiItemResolver[] MultiItemResolvers { get; set; } = Array.Empty<IMultiItemResolver>();

        /// <summary>
        /// Gets or sets the list of entity resolution ignore rules.
        /// </summary>
        /// <value>The entity resolution ignore rules.</value>
        private IResolverIgnoreRule[] EntityResolutionIgnoreRules { get; set; } = Array.Empty<IResolverIgnoreRule>();
        public void AddParts(
            IEnumerable<IResolverIgnoreRule> rules,
            IEnumerable<IItemResolver> resolvers)
        {
            EntityResolutionIgnoreRules = rules.ToArray();
            ItemResolvers = resolvers.OrderBy(i => i.Priority).ToArray();
            MultiItemResolvers = ItemResolvers.OfType<IMultiItemResolver>().ToArray();
        }

        public Person GetPerson(string name)
        {
            throw new NotImplementedException();
        }
        public Guid GetNewItemId(string key, Type type)
        {
            return GetNewItemIdInternal(key, type, false);
        }

        private Guid GetNewItemIdInternal(string key, Type type, bool forceCaseInsensitive)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            string programDataPath = _configurationManager.ApplicationPaths.ProgramDataPath;
            if (key.StartsWith(programDataPath, StringComparison.Ordinal))
            {
                // Try to normalize paths located underneath program-data in an attempt to make them more portable
                key = key.Substring(programDataPath.Length)
                    .TrimStart('/', '\\')
                    .Replace('/', '\\');
            }

            if (forceCaseInsensitive || !_configurationManager.CommonConfiguration.EnableCaseSensitiveItemIds)
            {
                key = key.ToLowerInvariant();
            }

            key = type.FullName + key;

            return key.GetMD5();
        }

        public BaseItem ResolvePath(FileSystemMetadata fileInfo, Folder parent = null, IDirectoryService directoryService = null)
            => ResolvePath(fileInfo, directoryService ?? new DirectoryService(_fileSystem), null, parent);

        private BaseItem ResolvePath(
           FileSystemMetadata fileInfo,
           IDirectoryService directoryService,
           IItemResolver[] resolvers,
           Folder parent = null,
           string collectionType = null,
           LibraryOptions libraryOptions = null)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            var fullPath = fileInfo.FullName;

            if (string.IsNullOrEmpty(collectionType) && parent != null)
            {
                collectionType = GetContentTypeOverride(fullPath, true);
            }

            var args = new ItemResolveArgs(_configurationManager.ApplicationPaths, directoryService)
            {
                Parent = parent,
                FileInfo = fileInfo,
                CollectionType = collectionType
            };

            // Return null if ignore rules deem that we should do so
            if (IgnoreFile(args.FileInfo, args.Parent))
            {
                return null;
            }

            // Gather child folder and files
            if (args.IsDirectory)
            {
                var isPhysicalRoot = args.IsPhysicalRoot;

                // When resolving the root, we need it's grandchildren (children of user views)
                var flattenFolderDepth = isPhysicalRoot ? 2 : 0;

                FileSystemMetadata[] files;
                var isVf = args.IsVf;

                try
                {
                    files = FileData.GetFilteredFileSystemEntries(directoryService, args.Path, _fileSystem, _appHost, _logger, args, flattenFolderDepth: flattenFolderDepth, resolveShortcuts: isPhysicalRoot || isVf);
                }
                catch (Exception ex)
                {
                    if (parent != null && parent.IsPhysicalRoot)
                    {
                        _logger.LogError(ex, "Error in GetFilteredFileSystemEntries isPhysicalRoot: {0} IsVf: {1}", isPhysicalRoot, isVf);

                        files = Array.Empty<FileSystemMetadata>();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Need to remove subpaths that may have been resolved from shortcuts
                // Example: if \\server\movies exists, then strip out \\server\movies\action
                if (isPhysicalRoot)
                {
                    files = NormalizeRootPathList(files).ToArray();
                }

                args.FileSystemChildren = files;
            }

            // Check to see if we should resolve based on our contents
            if (args.IsDirectory && !ShouldResolvePathContents(args))
            {
                return null;
            }

            return ResolveItem(args, resolvers);
        }

        /// <summary>
        /// Resolves the item.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="resolvers">The resolvers.</param>
        /// <returns>BaseItem.</returns>
        private BaseItem ResolveItem(ItemResolveArgs args, IItemResolver[] resolvers)
        {
            var item = (resolvers ?? ItemResolvers).Select(r => Resolve(args, r))
                .FirstOrDefault(i => i != null);

            if (item != null)
            {
                ResolverHelper.SetInitialItemValues(item, args, _fileSystem, this);
            }

            return item;
        }

        private BaseItem Resolve(ItemResolveArgs args, IItemResolver resolver)
        {
            try
            {
                return resolver.ResolvePath(args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Resolver} resolving {Path}", resolver.GetType().Name, args.Path);
                return null;
            }
        }

        public IEnumerable<BaseItem> ResolvePaths(IEnumerable<FileSystemMetadata> files, IDirectoryService directoryService, Folder parent, LibraryOptions libraryOptions, string collectionType = null)
        {
            return ResolvePaths(files, directoryService, parent, libraryOptions, collectionType, ItemResolvers);
        }

        public IEnumerable<BaseItem> ResolvePaths(
            IEnumerable<FileSystemMetadata> files,
            IDirectoryService directoryService,
            Folder parent,
            LibraryOptions libraryOptions,
            string collectionType,
            IItemResolver[] resolvers)
        {
            var fileList = files.Where(i => !IgnoreFile(i, parent)).ToList();

            if (parent != null)
            {
                var multiItemResolvers = resolvers == null ? MultiItemResolvers : resolvers.OfType<IMultiItemResolver>().ToArray();

                foreach (var resolver in multiItemResolvers)
                {
                    var result = resolver.ResolveMultiple(parent, fileList, collectionType, directoryService);

                    if (result?.Items.Count > 0)
                    {
                        var items = result.Items;
                        foreach (var item in items)
                        {
                            ResolverHelper.SetInitialItemValues(item, parent, this, directoryService);
                        }

                        items.AddRange(ResolveFileList(result.ExtraFiles, directoryService, parent, collectionType, resolvers, libraryOptions));
                        return items;
                    }
                }
            }

            return ResolveFileList(fileList, directoryService, parent, collectionType, resolvers, libraryOptions);
        }

        private IEnumerable<BaseItem> ResolveFileList(
            IReadOnlyList<FileSystemMetadata> fileList,
            IDirectoryService directoryService,
            Folder parent,
            string collectionType,
            IItemResolver[] resolvers,
            LibraryOptions libraryOptions)
        {
            // Given that fileList is a list we can save enumerator allocations by indexing
            for (var i = 0; i < fileList.Count; i++)
            {
                var file = fileList[i];
                BaseItem result = null;
                try
                {
                    result = ResolvePath(file, directoryService, resolvers, parent, collectionType, libraryOptions);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resolving path {Path}", file.FullName);
                }

                if (result != null)
                {
                    yield return result;
                }
            }
        }

        /// <summary>
        /// Determines whether a path should be ignored based on its contents - called after the contents have been read.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool ShouldResolvePathContents(ItemResolveArgs args)
        {
            // Ignore any folders containing a file called .ignore
            return !args.ContainsFileSystemEntryByName(".ignore");
        }

        public List<FileSystemMetadata> NormalizeRootPathList(IEnumerable<FileSystemMetadata> paths)
        {
            var originalList = paths.ToList();

            var list = originalList.Where(i => i.IsDirectory)
                .Select(i => _fileSystem.NormalizePath(i.FullName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var dupes = list.Where(subPath => !subPath.EndsWith(":\\", StringComparison.OrdinalIgnoreCase) && list.Any(i => _fileSystem.ContainsSubPath(i, subPath)))
                .ToList();

            foreach (var dupe in dupes)
            {
                _logger.LogInformation("Found duplicate path: {0}", dupe);
            }

            var newList = list.Except(dupes, StringComparer.OrdinalIgnoreCase).Select(_fileSystem.GetDirectoryInfo).ToList();
            newList.AddRange(originalList.Where(i => !i.IsDirectory));
            return newList;
        }

        private string GetContentTypeOverride(string path, bool inherit)
        {
            var nameValuePair = _configurationManager.CommonConfiguration.ContentTypes
                                    .FirstOrDefault(i => _fileSystem.AreEqual(i.Name, path)
                                                         || (inherit && !string.IsNullOrEmpty(i.Name)
                                                                     && _fileSystem.ContainsSubPath(i.Name, path)));
            return nameValuePair?.Value;
        }

        public bool IgnoreFile(FileSystemMetadata file, BaseItem parent)
            => EntityResolutionIgnoreRules.Any(r => r.ShouldIgnore(file, parent));

        public IEnumerable<BaseItem> FindExtras(BaseItem owner, IReadOnlyList<FileSystemMetadata> fileSystemChildren, IDirectoryService directoryService)
        {
            var ownerVideoInfo = VideoResolver.Resolve(owner.Path, owner.IsFolder, _namingOptions);
            if (ownerVideoInfo == null)
            {
                yield break;
            }

            var count = fileSystemChildren.Count;
            for (var i = 0; i < count; i++)
            {
                var current = fileSystemChildren[i];
                if (current.IsDirectory && _namingOptions.AllExtrasTypesFolderNames.ContainsKey(current.Name))
                {
                    var filesInSubFolder = _fileSystem.GetFiles(current.FullName, null, false, false);
                    foreach (var file in filesInSubFolder)
                    {
                        if (!_extraResolver.TryGetExtraTypeForOwner(file.FullName, ownerVideoInfo, out var extraType))
                        {
                            continue;
                        }

                        var extra = GetExtra(file, extraType.Value);
                        if (extra != null)
                        {
                            yield return extra;
                        }
                    }
                }
                else if (!current.IsDirectory && _extraResolver.TryGetExtraTypeForOwner(current.FullName, ownerVideoInfo, out var extraType))
                {
                    var extra = GetExtra(current, extraType.Value);
                    if (extra != null)
                    {
                        yield return extra;
                    }
                }
            }

            BaseItem GetExtra(FileSystemMetadata file, ExtraType extraType)
            {
                var extra = ResolvePath(_fileSystem.GetFileInfo(file.FullName), directoryService, _extraResolver.GetResolversForExtraType(extraType));
                if (extra is not Video)
                {
                    return null;
                }

                //// Try to retrieve it from the db. If we don't find it, use the resolved version
                //var itemById = GetItemById(extra.Id);
                //if (itemById != null)
                //{
                //    extra = itemById;
                //}

                extra.ExtraType = extraType;
                extra.ParentId = Guid.Empty;
                extra.OwnerId = owner.Id;
                return extra;
            }
        }
    }
}
