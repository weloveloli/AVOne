// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Facade
{
    using AVOne.Configuration;
    using AVOne.Constants;
    using AVOne.IO;
    using AVOne.Library;
    using AVOne.Models.Result;
    using AVOne.Providers;
    using Microsoft.Extensions.Logging;

    public class ToolFacade : IToolFacade
    {
        private readonly ILogger<ToolFacade> _logger;
        private readonly IProviderManager _providerManager;
        private readonly ILibraryManager _libraryManager;
        private readonly IConfigurationManager _configurationManager;
        private readonly IFileSystem _fileSystem;
        private readonly IDirectoryService _directoryService;

        public ToolFacade(ILogger<ToolFacade> logger, IProviderManager providerManager, ILibraryManager libraryManager, IConfigurationManager configurationManager, IFileSystem fileSystem, IDirectoryService directoryService)
        {
            _logger = logger;
            _providerManager = providerManager;
            _libraryManager = libraryManager;
            _configurationManager = configurationManager;
            _fileSystem = fileSystem;
            _directoryService = directoryService;
        }

        public IEnumerable<MetadataResult<T>> GetFileMetaDataByPath<T>(string filePath, string collectionType = CollectionType.PornMovies)
        {
            throw new NotImplementedException();
        }
    }
}
