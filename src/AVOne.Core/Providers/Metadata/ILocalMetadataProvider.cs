﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Providers.Metadata
{
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.IO;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Models.Result;

    public interface ILocalMetadataProvider : IMetadataProvider
    {
    }

    public interface ILocalMetadataProvider<TItemType> : IMetadataProvider<TItemType>, ILocalMetadataProvider
         where TItemType : BaseItem
    {
        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="directoryService">The directory service.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{MetadataResult{0}}.</returns>
        Task<MetadataResult<TItemType>> GetMetadata(
            ItemInfo info,
            IDirectoryService directoryService,
            CancellationToken cancellationToken);
    }
}
