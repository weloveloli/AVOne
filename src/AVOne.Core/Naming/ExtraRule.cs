// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Naming
{
    using AVOne.Enum;

    /// <summary>
    /// A rule used to match a file path with an <see cref="AVOne.Enum.ExtraType"/>.
    /// </summary>
    public class ExtraRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraRule"/> class.
        /// </summary>
        /// <param name="extraType">Type of extra.</param>
        /// <param name="ruleType">Type of rule.</param>
        /// <param name="token">Token.</param>
        /// <param name="mediaType">Media type.</param>
        public ExtraRule(ExtraType extraType, ExtraRuleType ruleType, string token, MediaType mediaType)
        {
            Token = token;
            ExtraType = extraType;
            RuleType = ruleType;
            MediaType = mediaType;
        }

        /// <summary>
        /// Gets or sets the token to use for matching against the file path.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the type of the extra to return when matched.
        /// </summary>
        public ExtraType ExtraType { get; set; }

        /// <summary>
        /// Gets or sets the type of the rule.
        /// </summary>
        public ExtraRuleType RuleType { get; set; }

        /// <summary>
        /// Gets or sets the type of the media to return when matched.
        /// </summary>
        public MediaType MediaType { get; set; }
    }
}
