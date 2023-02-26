// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Configuration
{
#nullable disable

    using System;

    /// <summary>
    /// Describes a single entry in the application configuration.
    /// </summary>
    public class ConfigurationStore
    {
        /// <summary>
        /// Gets or sets the unique identifier for the configuration.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the type used to store the data for this configuration entry.
        /// </summary>
        public Type ConfigurationType { get; set; }
    }
}
