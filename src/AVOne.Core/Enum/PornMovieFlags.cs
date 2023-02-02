// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Enum
{
    [Flags]
    public enum PornMovieFlags
    {
        None = 0b_0000_0000,  // 0
        Uncensored = 0b_0000_0001,  // 1
        ChineaseSubtilte = 0b_0000_0010,  // 2
        Hack = 0b_0000_0100,  // 4
        Thursday = 0b_0000_1000,  // 8
        Friday = 0b_0001_0000,  // 16
        Saturday = 0b_0010_0000,  // 32
        Sunday = 0b_0100_0000,  // 64
        Weekend = Saturday | Sunday
    }
}
