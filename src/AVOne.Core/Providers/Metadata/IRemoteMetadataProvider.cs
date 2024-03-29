﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Providers.Metadata
{
    using AVOne.Abstraction;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Models.Result;

    public interface IRemoteMetadataProvider : IMetadataProvider
    {
    }

    public interface IRemoteMetadataProvider<TItemType, in TLookupInfoType> : IMetadataProvider<TItemType>, IRemoteMetadataProvider, IRemoteMetadataSearchProvider<TLookupInfoType>
        where TItemType : BaseItem, IHasLookupInfo<TLookupInfoType>
        where TLookupInfoType : ItemLookupInfo, new()
    {
        Task<MetadataResult<TItemType>> GetMetadata(TLookupInfoType info, CancellationToken cancellationToken);
    }
}
