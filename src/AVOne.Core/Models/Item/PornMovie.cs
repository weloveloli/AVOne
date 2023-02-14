// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Models.Item
{
    using AVOne.Abstraction;
    using AVOne.Models.Info;

    public class PornMovie : Video, IHasLookupInfo<PornMovieInfo>
    {
        public PornMovieInfo PornMovieInfo => PornMovieInfo.Parse(this, ConfigurationManager.CommonConfiguration);

        PornMovieInfo IHasLookupInfo<PornMovieInfo>.GetLookupInfo() => PornMovieInfo;
    }
}
