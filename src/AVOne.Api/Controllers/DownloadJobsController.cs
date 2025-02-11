// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Api.Controllers
{
    using System.ComponentModel.DataAnnotations;
    using AVOne.Api.Attributes;
    using AVOne.Common;
    using AVOne.Impl.Data;
    using AVOne.Impl.Job;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Controllers for DownloadJob
    /// </summary>
    public class DownloadJobsController : BaseApiController
    {
        /// <summary>
        /// Job Repository
        /// </summary>
        private readonly JobRepository _jobRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadJobsController"/> class.
        /// </summary>
        /// <param name="jobRepository">The job repository.</param>
        public DownloadJobsController(JobRepository jobRepository) { _jobRepository = jobRepository; }

        /// <summary>
        /// Gets the thumb image of the download job.
        /// </summary>
        /// <param name="jobKey">Job key.</param>
        /// <response code="200">Job image returned.</response>
        /// <returns>Thumb image of the download job.</returns>
        [HttpGet("{jobKey}/Thumb")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesImageFile]
        [AllowAnonymous]
        public ActionResult GetThumb([FromRoute, Required] string jobKey)
        {
            var job = _jobRepository.GetJobByKey(jobKey);
            if (job == null)
            {
                return NotFound();
            }

            var exisit = DownloadAVJob.GetThumb(job, out var imagePath);
            if (!exisit || imagePath is null)
            {
                return NotFound();
            }
            return PhysicalFile(imagePath, MimeTypes.GetMimeType(imagePath));
        }

    }
}
