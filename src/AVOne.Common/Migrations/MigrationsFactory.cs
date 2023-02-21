// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Migrations
{
    using AVOne.Configuration;

    /// <summary>
    /// A factory that can find a persistent file of the migration configuration, which lists all applied migrations.
    /// </summary>
    public class MigrationsFactory : IConfigurationFactory
    {
        /// <inheritdoc/>
        public IEnumerable<ConfigurationStore> GetConfigurations()
        {
            return new[]
            {
                new MigrationsListStore()
            };
        }
    }
}
