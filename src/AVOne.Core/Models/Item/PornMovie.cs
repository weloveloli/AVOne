// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Models.Item
{
    using AVOne.Abstraction;
    using AVOne.Models.Info;

    public class PornMovie : Video, IHasLookupInfo<PornMovieInfo>
    {
        private PornMovieInfo? _pornMovieInfo;
        public PornMovieInfo PornMovieInfo
        {
            get
            {
                if (_pornMovieInfo is not null)
                {
                    return _pornMovieInfo;
                }
                else
                {
                    _pornMovieInfo = PornMovieInfo.Parse(this, ConfigurationManager.CommonConfiguration);
                }
                return _pornMovieInfo;
            }
            set
            {
                _pornMovieInfo = value;
            }
        }

        public override PornMovieInfo GetLookupInfo() => PornMovieInfo;
    }
}
