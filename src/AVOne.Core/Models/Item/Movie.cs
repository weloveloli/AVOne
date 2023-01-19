// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

#nullable disable

namespace AVOne.Models.Item
{
    using AVOne.Abstraction;
    using AVOne.Models.Info;

    public class Movie : Video, IHasLookupInfo<MovieInfo>
    {
        MovieInfo IHasLookupInfo<MovieInfo>.GetLookupInfo()
        {
            throw new NotImplementedException();
        }
    }
}
