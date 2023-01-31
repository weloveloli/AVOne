// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers
{
    using System.Text.RegularExpressions;
    using AVOne.Naming;

    public interface INamingOptionProvider : IProvider
    {
        INamingOptions GetNamingOption();
    }

    public interface INamingOptions
    {
        /// <summary>
        /// Gets or sets list of video file extensions.
        /// </summary>
        string[] VideoFileExtensions { get; }

        /// <summary>
        /// Gets or sets list of video flag delimiters.
        /// </summary>
        char[] VideoFlagDelimiters { get; }

        /// <summary>
        /// Gets list of clean datetime regular expressions.
        /// </summary>
        Regex[] CleanDateTimeRegexes { get; }

        /// <summary>
        /// Gets list of clean string regular expressions.
        /// </summary>
        Regex[] CleanStringRegexes { get; }

        /// <summary>
        /// Gets or sets list of episode regular expressions.
        /// </summary>
        EpisodeExpression[] EpisodeExpzressions { get; }

        /// <summary>
        /// Gets or sets list of multi-episode regular expressions.
        /// </summary>
        EpisodeExpression[] MultipleEpisodeExpressions { get; }

        /// <summary>
        /// Gets list of episode without season regular expressions.
        /// </summary>
        Regex[] EpisodeWithoutSeasonRegexes { get; }

        /// <summary>
        /// Gets list of multi-part episode regular expressions.
        /// </summary>
        Regex[] EpisodeMultiPartRegexes { get; }

        /// <summary>
        /// Gets or sets list of extra rules for videos.
        /// </summary>
        ExtraRule[] VideoExtraRules { get; }
    }
}
