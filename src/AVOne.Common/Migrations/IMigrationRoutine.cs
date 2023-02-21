// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Migrations
{
    /// <summary>
    /// Interface that describes a migration routine.
    /// </summary>
    public interface IMigrationRoutine
    {
        /// <summary>
        /// Gets the unique id for this migration. This should never be modified after the migration has been created.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the display name of the migration.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether to perform migration on a new install.
        /// </summary>
        public bool PerformOnNewInstall { get; }

        /// <summary>
        /// Execute the migration routine.
        /// </summary>
        public void Perform();
    }
}
