// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable
namespace AVOne.Models.Download
{
    public abstract class BaseDownloadableItem
    {
        public string SaveName { get; set; }
        public abstract string DisplayName { get; }
    }
}
