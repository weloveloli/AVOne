﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers
{
    public interface INamingOptionProvider : IProvider
    {
        INamingOption GetNamingOption();
    }

    public interface INamingOption
    {

    }
}
