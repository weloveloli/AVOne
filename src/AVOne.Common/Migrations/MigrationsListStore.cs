// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Migrations
{
    using AVOne.Configuration;

    /// <summary>
    /// A configuration that lists all the migration routines that were applied.
    /// </summary>
    public class MigrationsListStore : ConfigurationStore
    {
        /// <summary>
        /// The name of the configuration in the storage.
        /// </summary>
        public static readonly string StoreKey = "migrations";

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationsListStore"/> class.
        /// </summary>
        public MigrationsListStore()
        {
            ConfigurationType = typeof(MigrationOptions);
            Key = StoreKey;
        }
    }
}
