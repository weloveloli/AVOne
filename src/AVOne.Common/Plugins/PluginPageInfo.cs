﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Plugins
{
    /// <summary>
    /// Defines the <see cref="PluginPageInfo" />.
    /// </summary>
    public class PluginPageInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the resource path.
        /// </summary>
        public string EmbeddedResourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this plugin should appear in the main menu.
        /// </summary>
        public bool EnableInMainMenu { get; set; }

        /// <summary>
        /// Gets or sets the menu section.
        /// </summary>
        public string? MenuSection { get; set; }

        /// <summary>
        /// Gets or sets the menu icon.
        /// </summary>
        public string? MenuIcon { get; set; }
    }
}
