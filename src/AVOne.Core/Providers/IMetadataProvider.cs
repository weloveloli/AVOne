// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers
{
    using AVOne.Abstraction;
    using AVOne.Entities;

    /// <summary>
    /// Marker interface.
    /// </summary>
    public interface IMetadataProvider : IProvider
    {

    }

    public interface IMetadataProvider<TItemType> : IMetadataProvider
           where TItemType : BaseItem
    {
    }
}
