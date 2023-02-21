// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Resolvers
{
    using AVOne.Enum;
    using AVOne.Naming;

    /// <summary>
    /// Holder object for passing results from ExtraResolver.
    /// </summary>
    public class ExtraResult
    {
        /// <summary>
        /// Gets or sets the type of the extra.
        /// </summary>
        /// <value>The type of the extra.</value>
        public ExtraType? ExtraType { get; set; }

        /// <summary>
        /// Gets or sets the rule.
        /// </summary>
        /// <value>The rule.</value>
        public ExtraRule? Rule { get; set; }
    }
}
