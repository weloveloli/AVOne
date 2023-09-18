// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Api.Controllers
{
    using System.ComponentModel.DataAnnotations;
    using System.Net.Mime;
    using AVOne.Abstraction;
    using AVOne.Api.Attributes;
    using AVOne.Configuration;
    using AVOne.Constants;
    using AVOne.Impl.Facade;
    using AVOne.IO;
    using AVOne.Models.Systems;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Controllers for system
    /// </summary>
    /// <seealso cref="AVOne.Api.Controllers.BaseApiController" />
    public class SystemController : BaseApiController
    {
        private readonly IApplicationPaths _appPaths;
        private readonly IApplicationHost _appHost;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<SystemController> _logger;
        private readonly ISystemService _systemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemController"/> class.
        /// </summary>
        /// <param name="serverConfigurationManager">Instance of <see cref="IConfigurationManager"/> interface.</param>
        /// <param name="appHost">Instance of <see cref="IApplicationHost"/> interface.</param>
        /// <param name="fileSystem">Instance of <see cref="IFileSystem"/> interface.</param>
        /// <param name="logger">Instance of <see cref="ILogger{SystemController}"/> interface.</param>
        /// <param name="systemService">Instance of <see cref="ISystemService"/> interface.</param>
        public SystemController(
            IConfigurationManager serverConfigurationManager,
            IApplicationHost appHost,
            IFileSystem fileSystem,
            ILogger<SystemController> logger,
            ISystemService systemService)
        {
            _appPaths = serverConfigurationManager.ApplicationPaths;
            _appHost = appHost;
            _fileSystem = fileSystem;
            _logger = logger;
            _systemService = systemService;
        }

        /// <summary>
        /// Restarts the application.
        /// </summary>
        /// <response code="204">Server restarted.</response>
        /// <returns>No content. Server restarted.</returns>
        [HttpPost("Restart")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult RestartApplication()
        {
            Task.Run(async () =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                _appHost.Restart();
            });
            return NoContent();
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        /// <response code="204">Server shut down.</response>
        /// <returns>No content. Server shut down.</returns>
        [HttpPost("Shutdown")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult ShutdownApplication()
        {
            Task.Run(async () =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                await _appHost.Shutdown().ConfigureAwait(false);
            });
            return NoContent();
        }

        /// <summary>
        /// Gets a list of available server log files.
        /// </summary>
        /// <response code="200">Information retrieved.</response>
        /// <returns>An array of <see cref="LogFile"/> with the available log files.</returns>
        [HttpGet("Logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<LogFile[]> GetServerLogs()
        {
            return _systemService.GetServerLogs();
        }

        /// <summary>
        /// Gets a log file.
        /// </summary>
        /// <param name="name">The name of the log file to get.</param>
        /// <response code="200">Log file retrieved.</response>
        /// <returns>The log file.</returns>
        [HttpGet("Logs/Log")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesFile(MediaTypeNames.Text.Plain)]
        public ActionResult GetLogFile([FromQuery, Required] string name)
        {
            var file = _fileSystem.GetFiles(_appPaths.LogDirectoryPath)
                .First(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));

            // For older files, assume fully static
            var fileShare = file.LastWriteTimeUtc < DateTime.UtcNow.AddHours(-1) ? FileShare.Read : FileShare.ReadWrite;
            FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, fileShare, AVOneConstants.FileStreamBufferSize, FileOptions.Asynchronous);
            return File(stream, "text/plain; charset=utf-8");
        }
    }
}
