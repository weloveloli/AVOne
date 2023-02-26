// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#pragma warning disable CS1591

namespace AVOne.Resolvers
{
    using AVOne.Enum;
    using AVOne.IO;
    using AVOne.Models.Item;

    /// <summary>
    /// Interface IItemResolver.
    /// </summary>
    public interface IItemResolver
    {
        /// <summary>
        /// Gets the priority.
        /// </summary>
        /// <value>The priority.</value>
        ResolverPriority Priority { get; }

        /// <summary>
        /// Resolves the path.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>BaseItem.</returns>
        BaseItem ResolvePath(ItemResolveArgs args);
    }

    public interface IMultiItemResolver
    {
        MultiItemResolverResult ResolveMultiple(
            Folder parent,
            List<FileSystemMetadata> files,
            string collectionType,
            IDirectoryService directoryService);
    }

    public class MultiItemResolverResult
    {
        public MultiItemResolverResult()
        {
            Items = new List<BaseItem>();
            ExtraFiles = new List<FileSystemMetadata>();
        }

        public List<BaseItem> Items { get; set; }

        public List<FileSystemMetadata> ExtraFiles { get; set; }
    }
}
