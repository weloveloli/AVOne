// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#pragma warning disable CS1591

namespace AVOne.Common.Plugins
{
    using System;

    public interface IPluginAssembly
    {
        void SetAttributes(string assemblyFilePath, string dataFolderPath, Version assemblyVersion);

        void SetId(Guid assemblyId);
    }
}
