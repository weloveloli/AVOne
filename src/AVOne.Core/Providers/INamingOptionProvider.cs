// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers
{
    using System.Text.RegularExpressions;
    using AVOne.Abstraction;
    using AVOne.Enum;
    using AVOne.Naming;

    public interface INamingOptionProvider : IProvider, IHasOrder
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
        /// Gets or sets list of video stub file extensions.
        /// </summary>
        public string[] StubFileExtensions { get; }

        /// <summary>
        /// Gets or sets list of audio file extensions.
        /// </summary>
        public string[] AudioFileExtensions { get; }

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

        /// <summary>
        /// Gets the file stacking rules.
        /// </summary>
        FileStackRule[] VideoFileStackingRules { get; }

        /// <summary>
        /// Gets or sets the folder name to extra types mapping.
        /// </summary>
        public Dictionary<string, ExtraType> AllExtrasTypesFolderNames { get; }

        /// <summary>
        /// Gets or sets list of stub type rules.
        /// </summary>
        public StubTypeRule[] StubTypes { get; }
    }
}
