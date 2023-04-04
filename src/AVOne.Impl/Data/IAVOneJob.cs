// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Impl.Data
{
    using System;
    using System.Collections.Generic;
    using AVOne.Abstraction;
    using AVOne.Impl.Job;
    using LiteDB;

    /// <summary>
    /// Defines the <see cref="IAVOneJob" />.
    /// </summary>
    public abstract class IAVOneJob
    {
        public static IApplicationHost ApplicationHost { get; set; }
        public static IJobManager JobManager { get; set; }
        public IProgress<double> Progress { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IAVOneJob"/> class.
        /// </summary>
        public IAVOneJob()
        {
            this.Created = DateTime.UtcNow;
            this.Modified = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; }

        /// <summary>
        /// Gets the Type
        /// Gets or sets the Type...
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the Modified.
        /// </summary>
        public DateTime Modified { get; set; }

        /// <summary>
        /// Gets or sets the Created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets the progress value.
        /// </summary>
        /// <value>
        /// The progress value.
        /// </value>
        public double ProgressValue { get; set; }

        /// <summary>
        /// Gets or sets the Key.
        /// </summary>
        public abstract string Key { get; }

        /// <summary>
        /// The ToModel.
        /// </summary>
        /// <returns>The <see cref="JobModel"/>.</returns>
        public virtual JobModel ToModel()
        {
            return new JobModel
            {
                Id = this.Id,
                Status = this.Status,
                Modified = this.Modified,
                Created = this.Created,
                Extra = this.BuildExtra(),
                Type = this.Type,
                Key = this.Key,
                Name = this.Name,
                Description = this.Description,
                Progress = this.ProgressValue
            };
        }

        /// <summary>
        /// The FromModel.
        /// </summary>
        /// <param name="model">The model<see cref="JobModel"/>.</param>
        public virtual void FromModel(JobModel model)
        {
            this.Id = model.Id;
            this.Status = model.Status;
            this.Modified = model.Modified;
            this.Created = model.Created;
            this.Name = model.Name;
            this.Description = model.Description;
            this.ProgressValue = model.Progress;
            FromExtra(model.Extra);
        }

        /// <summary>
        /// The BuildExtra.
        /// </summary>
        /// <returns>The <see cref="Dictionary{string, string}"/>.</returns>
        protected abstract Dictionary<string, string> BuildExtra();

        /// <summary>
        /// The BuildExtra.
        /// </summary>
        /// <param name="extra">The extra<see cref="Dictionary{string, string}"/>.</param>
        protected abstract void FromExtra(Dictionary<string, string> extra);

        public abstract Task Execute(CancellationToken cancellationToken);
    }
}
