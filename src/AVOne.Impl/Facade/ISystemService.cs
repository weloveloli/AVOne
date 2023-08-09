// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Facade
{
    using AVOne.Models.Systems;

    /// <summary>
    /// System service interface
    /// </summary>
    public interface ISystemService
    {
        public LogFile[] GetServerLogs();
    }
}
