// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Job
{
    using AVOne.Impl.Data;

    public interface IJobManager
    {
        public void AddJob<T>(T job) where T : IAVOneJob;

        public Task ExecuteJob<T>(T job) where T : IAVOneJob;

        public void RemoveJob<T>(T job) where T : IAVOneJob;

        public void CancelJob<T>(T job) where T : IAVOneJob;
    }
}
