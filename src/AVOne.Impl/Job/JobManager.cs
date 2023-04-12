// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Job
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using AVOne.Impl.Data;
    using AVOne.Models.Job;
    using LiteDB;
    using Microsoft.Extensions.Logging;

    public class JobManager : IJobManager
    {
        private readonly IDictionary<string, Task> TaskInstances;
        private readonly IDictionary<string, CancellationTokenSource> CancelToken;
        private readonly JobRepository _jobRepository;
        private readonly ILogger<JobManager> _logger;

        public JobManager(JobRepository jobRepository, ILogger<JobManager> logger)
        {
            TaskInstances = new ConcurrentDictionary<string, Task>();
            CancelToken = new ConcurrentDictionary<string, CancellationTokenSource>();
            _jobRepository = jobRepository;
            _logger = logger;
        }

        public void AddJob<T>(T job) where T : IAVOneJob
        {
            job.Id = ObjectId.NewObjectId();
            _jobRepository.UpsertJob(job);
            job.Progress = CreateProcessForJob(job);
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

        public IProgress<JobStatusArgs> CreateProcessForJob(IAVOneJob job)
        {
            return new JobManagerProgress(_jobRepository, job);
        }

        public void CancelJob<T>(T job) where T : IAVOneJob
        {
            if (job == null)
            {
                throw new ArgumentNullException("Job is null");
            }

            if (CancelToken.TryGetValue(job.Key, out var tokenSource))
            {
                tokenSource.Cancel();
            }

            job.Status = JobStatus.Canceled;
        }

        public Task ExecuteJob<T>(T job) where T : IAVOneJob
        {
            job.Status = JobStatus.Running;
            _jobRepository.UpsertJob(job);
            var cancellationTokenSource = new CancellationTokenSource();
            CancelToken[job.Key] = cancellationTokenSource;
            var task = job.Execute(cancellationTokenSource.Token);
            TaskInstances[job.Key] = task;

            var exeTaks = task.ContinueWith((t) =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    job.Status = JobStatus.Completed;
                    job.ProgressValue = 100;
                }
                else if (t.IsCanceled)
                {
                    job.Status = JobStatus.Canceled;
                    _logger.LogDebug("Job {0} is Canceled", job.Key);
                }
                else if (t.IsFaulted)
                {
                    job.Status = JobStatus.Failed;
                    _logger.LogWarning(t.Exception, "Job {0} canceld due to exception", job.Key);
                }

                _jobRepository.UpsertJob(job);
                TaskInstances.Remove(job.Key);
                CancelToken.Remove(job.Key);
            });
            return exeTaks;
        }

        public void CancelJobByKey(string jobKey)
        {
            if (string.IsNullOrEmpty(jobKey))
            {
                return;
            }
            if (CancelToken.TryGetValue(jobKey, out var tokenSource))
            {
                tokenSource.Cancel();
            }
            else
            {
                var job = _jobRepository.GetJobByKey(jobKey);
                job.Status = JobStatus.Canceled;
                _jobRepository.UpsertJob(job);
            }
        }

        public void DeleteJob(string jobKey)
        {
            _jobRepository.DeleteJob(jobKey);
        }
    }

    public class JobManagerProgress : IProgress<JobStatusArgs>
    {
        private readonly JobRepository _jobRepository;

        private readonly IAVOneJob job;

        private double _progress = 0;

        private DateTime _lastEventTime = DateTime.UtcNow;

        public JobManagerProgress(JobRepository jobRepository, IAVOneJob job)
        {
            _jobRepository = jobRepository;
            this.job = job;
        }

        public void Report(JobStatusArgs args)
        {
            var value = args.Progress;
            job.UpdateStatus(args);
            if (_progress < value)
            {
                var now = DateTime.UtcNow;
                _progress = value;
                if (value >= 100 || now.Subtract(_lastEventTime).TotalMilliseconds >= 500)
                {
                    this._jobRepository.UpsertJob(job);
                    _lastEventTime = now;
                }
            }
        }
    }
}
