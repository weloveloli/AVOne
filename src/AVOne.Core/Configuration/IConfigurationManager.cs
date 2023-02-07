// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Configuration
{
    public interface IConfigurationManager
    {
        /// <summary>
        /// Saves the configuration.
        /// </summary>
        void SaveConfiguration();

        /// <summary>
        /// Gets the application paths.
        /// </summary>
        /// <value>The application paths.</value>
        IApplicationPaths CommonApplicationPaths { get; }

        IApplicationConfigs Configuration {get; }
    }
}
