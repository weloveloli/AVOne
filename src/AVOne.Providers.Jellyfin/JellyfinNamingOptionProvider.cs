// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Jellyfin
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using AVOne.Enum;
    using AVOne.Naming;
    using AVOne.Providers;

    public class JellyfinNamingOptionProvider : INamingOptionProvider
    {
        private readonly INamingOptions _namingOptions;
        public JellyfinNamingOptionProvider()
        {
            _namingOptions = new JellyfinNamingOptions();
        }

        public int Order => int.MaxValue;

        public INamingOptions GetNamingOption() => _namingOptions;
    }

    internal class JellyfinNamingOptions : INamingOptions
    {
        private readonly Emby.Naming.Common.NamingOptions _options;

        public JellyfinNamingOptions()
        {
            _options = new Emby.Naming.Common.NamingOptions();
        }

        /// <summary>
        /// Gets or sets list of video file extensions.
        /// </summary>
        public string[] VideoFileExtensions => _options.VideoFileExtensions;

        /// <summary>
        /// Gets or sets list of video stub file extensions.
        /// </summary>
        public string[] StubFileExtensions => _options.StubFileExtensions;

        /// <summary>
        /// Gets or sets list of video flag delimiters.
        /// </summary>
        public char[] VideoFlagDelimiters => _options.VideoFlagDelimiters;

        /// <summary>
        /// Gets list of clean datetime regular expressions.
        /// </summary>
        public Regex[] CleanDateTimeRegexes => _options.CleanDateTimeRegexes;

        /// <summary>
        /// Gets list of clean string regular expressions.
        /// </summary>
        public Regex[] CleanStringRegexes => _options.CleanStringRegexes;

        /// <summary>
        /// Gets or sets list of episode regular expressions.
        /// </summary>
        public EpisodeExpression[] EpisodeExpzressions => _options.EpisodeExpressions;

        /// <summary>
        /// Gets or sets list of multi-episode regular expressions.
        /// </summary>
        public EpisodeExpression[] MultipleEpisodeExpressions => _options.MultipleEpisodeExpressions;

        /// <summary>
        /// Gets list of episode without season regular expressions.
        /// </summary>
        public Regex[] EpisodeWithoutSeasonRegexes => _options.EpisodeWithoutSeasonRegexes;

        /// <summary>
        /// Gets list of multi-part episode regular expressions.
        /// </summary>
        public Regex[] EpisodeMultiPartRegexes => _options.EpisodeMultiPartRegexes;

        /// <summary>
        /// Gets or sets list of extra rules for videos.
        /// </summary>
        public ExtraRule[] VideoExtraRules => _options.VideoExtraRules;

        public FileStackRule[] VideoFileStackingRules => _options.VideoFileStackingRules;
        public Dictionary<string, ExtraType> AllExtrasTypesFolderNames => _options.AllExtrasTypesFolderNames;
        public StubTypeRule[] StubTypes => _options.StubTypes;

        /// <summary>
        /// Gets or sets list of audio file extensions.
        /// </summary>
        public string[] AudioFileExtensions => _options.AudioFileExtensions;
    }
}
