// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Plugins
{
    using System;

    /// <summary>
    /// Defines the <see cref="IHasPluginConfiguration" />.
    /// </summary>
    public interface IHasPluginConfiguration
    {
        /// <summary>
        /// Gets the type of configuration this plugin uses.
        /// </summary>
        Type ConfigurationType { get; }

        /// <summary>
        /// Gets the plugin's configuration.
        /// </summary>
        BasePluginConfiguration Configuration { get; }

        /// <summary>
        /// Completely overwrites the current configuration with a new copy.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        void UpdateConfiguration(BasePluginConfiguration configuration);
    }
}
