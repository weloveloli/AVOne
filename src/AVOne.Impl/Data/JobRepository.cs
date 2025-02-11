// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using LiteDB;

    public class JobRepository : RepositoryBase
    {
        /// <summary>
        /// Defines the downloadJobs.
        /// </summary>
        private readonly ILiteCollection<JobModel> Jobs;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        public JobRepository(ApplicationDbContext dbContext)
        {

            this.Jobs = dbContext.GetCollection<JobModel>("Jobs");
            this.Jobs.EnsureIndex(o => o.Key);
        }

        /// <summary>
        /// The IsExist.
        /// </summary>
        /// <param name="job">The job<see cref="IAVOneJob"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool IsExist(IAVOneJob job)
        {
            return this.Jobs.Exists(j => j.Type == job.Type && j.Key == job.Key);
        }

        /// <summary>
        /// The UpserJob.
        /// </summary>
        /// <param name="job">The job<see cref="IAVOneJob"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool UpsertJob(IAVOneJob job)
        {
            var model = job.ToModel();
            model.Modified = DateTime.UtcNow;
            return this.UpsertJob(model);
        }

        /// <summary>
        /// The UpserJob.
        /// </summary>
        /// <param name="job">The job<see cref="IAVOneJob"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool UpsertJob(JobModel job)
        {
            return this.Jobs.Upsert(job);
        }

        public JobModel GetJobByKey(string key)
        {
            return this.Jobs.FindOne(o => o.Key == key);
        }

        public void DeleteJob(string key)
        {
            var job = this.Jobs.FindOne(o => o.Key == key);
            if (job != null)
            {
                this.Jobs.Delete(job.Id);
            }
        }
        /// <summary>
        /// The GetJob.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="key">The key<see cref="string"/>.</param>
        /// <returns>The <see cref="T"/>.</returns>
        public T? GetJob<T>(string key) where T : IAVOneJob, new()
        {
            var t = new T();
            var job = this.Jobs.FindOne(j => j.Type == t.Type && j.Key == key);
            if (job == null)
            {
                return default;
            }
            t.FromModel(job);
            return t;
        }

        /// <summary>
        /// The GetUnfinishedJobs.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="predicate">The predicate<see cref="BsonExpression"/>.</param>
        /// <param name="skip">The skip<see cref="int"/>.</param>
        /// <param name="limit">The limit<see cref="int"/>.</param>
        /// <returns>The <see cref="List{T}"/>.</returns>
        public IEnumerable<JobModel> GetJobs(Expression<Func<JobModel, bool>> predicate, int skip = 0, int limit = int.MaxValue)
        {
            return this.Jobs.Find(predicate, skip, limit);
        }

        /// <summary>
        /// The GetUnfinishedJobs.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="predicate">The predicate<see cref="BsonExpression"/>.</param>
        /// <param name="skip">The skip<see cref="int"/>.</param>
        /// <param name="limit">The limit<see cref="int"/>.</param>
        /// <returns>The <see cref="List{T}"/>.</returns>
        public IEnumerable<JobModel> GetJobs(int skip = 0, int limit = int.MaxValue, params (bool condition, Expression<Func<JobModel, bool>> expression)[] conditionPredicates)
        {
            var predicate = this.MergeExp(conditionPredicates);
            return this.Jobs.Find(predicate, skip, limit);
        }
        /// <summary>
        /// The Count.
        /// </summary>
        /// <param name="conditionPredicates">The conditionPredicates.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public int Count(params (bool condition, Expression<Func<JobModel, bool>> expression)[] conditionPredicates)
        {
            var exp = this.MergeExp(conditionPredicates);
            return this.Jobs.Count(exp);
        }

        /// <summary>
        /// The GetPagedList.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="pageIndex">The pageIndex<see cref="int"/>.</param>
        /// <param name="pageSize">The pageSize<see cref="int"/>.</param>
        /// <param name="conditionPredicates">The predicate.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public PagedList<JobModel> GetPagedList(int pageIndex, int pageSize, params (bool condition, Expression<Func<JobModel, bool>> expression)[] conditionPredicates)
        {
            var totalCount = this.Count(conditionPredicates);
            var exp = this.MergeExp(conditionPredicates);
            var jobs = this.GetJobs(exp, (pageIndex - 1) * pageSize, pageSize);
            return BuildPageList(totalCount, pageIndex, pageSize, jobs);
        }
    }
}
