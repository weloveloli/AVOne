// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Configuration
{
    using MediaBrowser.Common.Configuration;

    public class AVOneConfigurationFactory : IConfigurationFactory
    {
        /// <inheritdoc/>
        public IEnumerable<ConfigurationStore> GetConfigurations()
        {
            return new[]
            {
                new AVOneConfigStore()
            };
        }
    }
}
