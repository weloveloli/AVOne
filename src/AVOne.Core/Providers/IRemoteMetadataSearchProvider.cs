// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Providers
{
    using AVOne.Models.Info;
    using AVOne.Models.Result;

    public interface IRemoteMetadataSearchProvider : IRemoteMetadataProvider
    {
    }

    public interface IRemoteMetadataSearchProvider<in TLookupInfoType> : IRemoteMetadataSearchProvider
        where TLookupInfoType : ItemLookupInfo
    {
        Task<IEnumerable<RemoteMetadataSearchResult>> GetSearchResults(TLookupInfoType searchInfo, CancellationToken cancellationToken);
    }
}
