// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Resolvers
{
    using AVOne.Configuration;
    using AVOne.IO;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using AVOne.Resolvers;

    /// <summary>
    /// Provides the core resolver ignore rules.
    /// </summary>
    public class CoreResolutionIgnoreRule : IResolverIgnoreRule
    {
        private readonly IProviderManager providerManager;
        private readonly IApplicationPaths _serverApplicationPaths;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreResolutionIgnoreRule"/> class.
        /// </summary>
        /// <param name="namingOptions">The naming options.</param>
        /// <param name="serverApplicationPaths">The server application paths.</param>
        public CoreResolutionIgnoreRule(IProviderManager _providerManager, IApplicationPaths serverApplicationPaths)
        {
            providerManager = _providerManager;
            _serverApplicationPaths = serverApplicationPaths;
        }

        /// <inheritdoc />
        public bool ShouldIgnore(FileSystemMetadata fileInfo, BaseItem parent)
        {
            var _namingOptions = providerManager.GetNamingOptionProvider().GetNamingOption();

            if (IgnorePatterns.ShouldIgnore(fileInfo.FullName))
            {
                return true;
            }

            var filename = fileInfo.Name;

            if (fileInfo.IsDirectory)
            {
                if (parent != null)
                {
                    // Ignore extras folders but allow it at the collection level
                    if (_namingOptions.AllExtrasTypesFolderNames.ContainsKey(filename))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
