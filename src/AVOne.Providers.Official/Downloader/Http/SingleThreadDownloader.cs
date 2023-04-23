// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Downloader.Http
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Configuration;
    using AVOne.Models.Download;
    using Microsoft.Extensions.Logging;

    internal class SingleThreadDownloader
    {
        private IConfigurationManager _configurationManager;
        private HttpClient _client;
        private ILogger<HttpDownloadProvider> _logger;
        private IApplicationPaths _applicationPaths;

        public SingleThreadDownloader(IConfigurationManager configurationManager, HttpClient client, ILogger<HttpDownloadProvider> logger, IApplicationPaths applicationPaths)
        {
            _configurationManager = configurationManager;
            _client = client;
            _logger = logger;
            _applicationPaths = applicationPaths;
        }

        internal Task CreateTask(BaseDownloadableItem item, DownloadOpts opts, string outputFile, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
