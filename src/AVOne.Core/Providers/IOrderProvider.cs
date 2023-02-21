// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers
{
    public interface IOrderProvider : IProvider
    {
        public int Order { get; }
    }
}
