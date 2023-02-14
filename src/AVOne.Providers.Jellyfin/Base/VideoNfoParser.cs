// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers.Jellyfin.Base
{
    using System.Xml;
    using AVOne.Configuration;
    using AVOne.IO;
    using AVOne.Models.Item;
    using AVOne.Models.Result;
    using AVOne.Providers;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Nfo parser for movies.
    /// </summary>
    public class VideoNfoParser : BaseNfoParser<Video>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoNfoParser"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
        /// <param name="config">Instance of the <see cref="IConfigurationManager"/> interface.</param>
        /// <param name="providerManager">Instance of the <see cref="IProviderManager"/> interface.</param>
        /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
        /// <param name="userDataManager">Instance of the <see cref="IUserDataManager"/> interface.</param>
        /// <param name="directoryService">Instance of the <see cref="DirectoryService"/> interface.</param>
        public VideoNfoParser(
            ILogger logger,
            IConfigurationManager config,
            IProviderManager providerManager,
            IDirectoryService directoryService)
            : base(logger, config, providerManager, directoryService)
        {
        }

        /// <inheritdoc />
        protected override bool SupportsUrlAfterClosingXmlTag => true;

        /// <inheritdoc />
        protected override void FetchDataFromXmlNode(XmlReader reader, MetadataResult<Video> itemResult)
        {
            _ = itemResult.Item;
        }
    }
}
