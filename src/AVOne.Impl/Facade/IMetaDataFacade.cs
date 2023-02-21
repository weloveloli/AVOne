// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Facade
{
    using AVOne.Impl.Models;

    public interface IMetaDataFacade
    {
        public Task<MoveMetaDataItem> ResolveAsMovie(string path, CancellationToken token = default);
        public Task<IEnumerable<MoveMetaDataItem>> ResolveAsMovies(string dir, CancellationToken token = default);
    }
}
