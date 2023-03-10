// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Facade
{
    using AVOne.Tool.Models;

    public interface IMetaDataFacade
    {
        public Task<MoveMetaDataItem> ResolveAsMovie(string path, CancellationToken token = default);
        public Task<IEnumerable<MoveMetaDataItem>> ResolveAsMovies(string dir, CancellationToken token = default);
        public Task SaveMetaDataToLocal(MoveMetaDataItem item, CancellationToken token = default);
    }
}
