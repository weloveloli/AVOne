// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Providers
{
    using AVOne.Models.Info;
    using AVOne.Models.Result;

    public interface IRemoteMetadataSearchProvider
    {
    }

    public interface IRemoteMetadataSearchProvider<in TLookupInfoType> : IRemoteMetadataSearchProvider
        where TLookupInfoType : ItemLookupInfo
    {
        Task<IEnumerable<RemoteMetadataSearchResult>> GetSearchResults(TLookupInfoType searchInfo, CancellationToken cancellationToken);
    }
}
