// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.MetaTube.Migrations.PreStartupRoutines
{

    using Microsoft.Extensions.Logging;
    using System.Xml.Serialization;
    using System.Xml;
    using AVOne.Configuration;
    using AVOne.Common.Migrations;
    using AVOne.Providers.MetaTube.Configuration;

    /// <inheritdoc />
    public class CreateMetaTubeConfiguration : IMigrationRoutine
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly ILogger<CreateMetaTubeConfiguration> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAVOneConfiguration"/> class.
        /// </summary>
        /// <param name="applicationPaths">An instance of <see cref="ServerApplicationPaths"/>.</param>
        /// <param name="loggerFactory">An instance of the <see cref="ILoggerFactory"/> interface.</param>
        public CreateMetaTubeConfiguration(IApplicationPaths applicationPaths, ILoggerFactory loggerFactory)
        {
            _applicationPaths = applicationPaths;
            _logger = loggerFactory.CreateLogger<CreateMetaTubeConfiguration>();
        }

        /// <inheritdoc />
        public Guid Id => Guid.Parse("B8F9C0E1-7A5D-4F3D-AE2C-6B0A9F6E8B7C");

        /// <inheritdoc />
        public string Name => nameof(CreateMetaTubeConfiguration);

        /// <inheritdoc />
        public bool PerformOnNewInstall => true;

        /// <inheritdoc />
        public void Perform()
        {
            var path = Path.Combine(_applicationPaths.ConfigurationDirectoryPath, $"{MetaTubeConfigStore.StoreKey}.xml");
            if (File.Exists(path))
            {
                _logger.LogDebug("AVOne configuration file already exists, skipping");
                return;
            }

            var serializer = new XmlSerializer(typeof(MetaTubeConfiguration), new XmlRootAttribute("MetaTubeConfiguration"));
            var xmlWriterSettings = new XmlWriterSettings { Indent = true };
            using var xmlWriter = XmlWriter.Create(path, xmlWriterSettings);
            serializer.Serialize(xmlWriter, new MetaTubeConfiguration());
        }
    }
}
