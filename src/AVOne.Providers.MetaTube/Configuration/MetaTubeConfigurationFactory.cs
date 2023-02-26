// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.MetaTube.Configuration
{
    using System.Collections.Generic;
    using AVOne.Configuration;

    public class MetaTubeConfigurationFactory : IConfigurationFactory
    {
        /// <inheritdoc/>
        public IEnumerable<ConfigurationStore> GetConfigurations()
        {
            return new[]
            {
                new MetaTubeConfigStore()
            };
        }
    }
}
