// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Models.Item
{
    using AVOne.Abstraction;
    using AVOne.Models.Info;
    using AVOne.Providers;

    public class PornMovie : Video, IHasLookupInfo<MovieInfo>
    {
        public MovieId MovieId { get; set; }

        MovieInfo IHasLookupInfo<MovieInfo>.GetLookupInfo()
        {
            throw new NotImplementedException();
        }
    }
}
