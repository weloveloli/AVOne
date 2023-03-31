// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Job
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using AVOne.Impl.Data;
    using LiteDB;

    public class JobManager : IJobManager
    {
        private readonly IDictionary<string, Task> TaskInstances;
        private readonly IDictionary<string, CancellationTokenSource> CancelToken;
        private readonly JobRepository _jobRepository;

        public JobManager(JobRepository jobRepository)
        {
            TaskInstances = new ConcurrentDictionary<string, Task>();
            CancelToken = new ConcurrentDictionary<string, CancellationTokenSource>();
            _jobRepository = jobRepository;
        }

        public void AddJob<T>(T job) where T : IAVOneJob
        {
            _jobRepository.UpsertJob(job);
        }

        /// <summary>
        /// The GetUnfinishedJobs.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="predicate">The predicate<see cref="BsonExpression"/>.</param>
        /// <param name="skip">The skip<see cref="int"/>.</param>
        /// <param name="limit">The limit<see cref="int"/>.</param>
        /// <returns>The <see cref="List{T}"/>.</returns>
        public IEnumerable<T> GetJobs<T>(Expression<Func<JobModel, bool>> predicate, int skip = 0, int limit = int.MaxValue) where T : IAVOneJob, new()
        {
            var jobModels = this._jobRepository.GetJobs(predicate, skip, limit);
            return jobModels.Select(j =>
            {
                var t = new T();
                t.FromModel(j);
                t.Progress = CreateProcessForJob(t);
                return t;
            }).ToList();
        }

        private IProgress<double> CreateProcessForJob(IAVOneJob job)
        {
            return new Progress<double>( value =>
            {
                job.ProgressValue = value;
                if (value >= 100)
                {
                    job.Status = (int)JobStatus.Completed;
                }
                this._jobRepository.UpsertJob(job);
            });
        }

        public void CancelJob<T>(T job) where T : IAVOneJob
        {
            if (job == null) throw new ArgumentNullException("Job is null");
            if (CancelToken.TryGetValue(job.Key, out var tokenSource))
            {
                tokenSource.Cancel();
            }

            job.Status = (int)JobStatus.Canceled;
            _jobRepository.UpsertJob(job);
        }

        public Task ExecuteJob<T>(T job) where T : IAVOneJob
        {
            var cancellationTokenSource = new CancellationTokenSource();
            CancelToken[job.Key] = cancellationTokenSource;
            var task = job.Execute(cancellationTokenSource.Token);
            TaskInstances[job.Key] = task;
            return task;
        }

        public void RemoveJob<T>(T job) where T : IAVOneJob
        {
            if (job == null) throw new ArgumentNullException("Job is null");
            if (CancelToken.TryGetValue(job.Key, out var tokenSource))
            {
                tokenSource.Cancel();
            }

            job.Status = (int)JobStatus.Deleted;
            _jobRepository.UpsertJob(job);
        }
    }
}
