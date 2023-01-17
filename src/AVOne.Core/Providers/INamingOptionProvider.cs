// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers
{
    public interface INamingOptionProvider : IProvider
    {
        INamingOptions GetNamingOption();
    }

    public interface INamingOptions
    {
        string[] VideoFileExtensions { get; }
    }
}
