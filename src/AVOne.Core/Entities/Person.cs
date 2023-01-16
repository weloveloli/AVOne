// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Entities
{
    using AVOne.Abstraction;
    using AVOne.Models.Info;

    public class Person : BaseItem, IHasLookupInfo<PersonLookupInfo>
    {
        public PersonLookupInfo GetLookupInfo() => GetItemLookupInfo<PersonLookupInfo>();
    }
}
