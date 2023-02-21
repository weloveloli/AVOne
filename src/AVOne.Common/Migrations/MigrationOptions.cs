// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Migrations
{
    /// <summary>
    /// Configuration part that holds all migrations that were applied.
    /// </summary>
    public class MigrationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationOptions"/> class.
        /// </summary>
        public MigrationOptions()
        {
            Applied = new List<(Guid Id, string Name)>();
        }

        // .Net xml serializer can't handle interfaces
#pragma warning disable CA1002 // Do not expose generic lists
        /// <summary>
        /// Gets the list of applied migration routine names.
        /// </summary>
        public List<(Guid Id, string Name)> Applied { get; }
#pragma warning restore CA1002
    }
}
