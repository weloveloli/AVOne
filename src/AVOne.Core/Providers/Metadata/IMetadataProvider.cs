﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Providers.Metadata
{
    using AVOne.Models.Item;
    using AVOne.Providers;

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
