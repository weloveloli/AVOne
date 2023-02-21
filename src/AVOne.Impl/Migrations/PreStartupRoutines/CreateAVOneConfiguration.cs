// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Migrations.PreStartupRoutines
{

    using Microsoft.Extensions.Logging;
    using System.Xml.Serialization;
    using System.Xml;
    using AVOne.Impl.Configuration;
    using AVOne.Configuration;
    using AVOne.Common.Migrations;

    /// <inheritdoc />
    public class CreateAVOneConfiguration : IMigrationRoutine
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly ILogger<CreateAVOneConfiguration> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAVOneConfiguration"/> class.
        /// </summary>
        /// <param name="applicationPaths">An instance of <see cref="ServerApplicationPaths"/>.</param>
        /// <param name="loggerFactory">An instance of the <see cref="ILoggerFactory"/> interface.</param>
        public CreateAVOneConfiguration(IApplicationPaths applicationPaths, ILoggerFactory loggerFactory)
        {
            _applicationPaths = applicationPaths;
            _logger = loggerFactory.CreateLogger<CreateAVOneConfiguration>();
        }

        /// <inheritdoc />
        public Guid Id => Guid.Parse("8A72C918-8F92-E2A2-6D45-BA4A4888AC6E");

        /// <inheritdoc />
        public string Name => nameof(CreateAVOneConfiguration);

        /// <inheritdoc />
        public bool PerformOnNewInstall => true;

        /// <inheritdoc />
        public void Perform()
        {
            var path = Path.Combine(_applicationPaths.ConfigurationDirectoryPath, $"{AVOneConfigStore.StoreKey}.xml");
            if (File.Exists(path))
            {
                _logger.LogDebug("AVOne configuration file already exists, skipping");
                return;
            }

            var serializer = new XmlSerializer(typeof(AVOneConfiguration), new XmlRootAttribute("AVOneConfiguration"));
            var xmlWriterSettings = new XmlWriterSettings { Indent = true };
            using var xmlWriter = XmlWriter.Create(path, xmlWriterSettings);
            serializer.Serialize(xmlWriter, new AVOneConfiguration());
        }
    }
}
