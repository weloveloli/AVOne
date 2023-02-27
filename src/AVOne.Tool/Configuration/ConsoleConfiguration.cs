// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool.Configuration
{
    using AVOne.Common.Plugins;
    using AVOne.Configuration;

    public class ConsoleConfiguration : BaseApplicationConfiguration, IPluginConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is first run.
        /// </summary>
        /// <value><c>true</c> if this instance is first run; otherwise, <c>false</c>.</value>
        public bool IsStartupWizardCompleted { get; set; }

        public bool RemoveOldPlugins => true;

        public ConsoleConfiguration()
        {
        }
    }
}
