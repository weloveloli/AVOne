// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers.Jellyfin
{
    using System.Text.RegularExpressions;
    using AVOne.Enum;
    using AVOne.Impl.Constants;
    using AVOne.Naming;
    using AVOne.Providers;

    public class JellyfinNamingOptionProvider : INamingOptionProvider
    {

        private readonly INamingOptions _namingOptions;
        public JellyfinNamingOptionProvider()
        {
            _namingOptions = new JellyfinNamingOptions();
        }

        public string Name => OfficialProviderNames.Jellifin;

        public INamingOptions GetNamingOption() => _namingOptions;
    }

    internal class JellyfinNamingOptions : INamingOptions
    {
        private readonly Emby.Naming.Common.NamingOptions _options;
        private readonly EpisodeExpression[] _multipleEpisodeExpressions;

        public JellyfinNamingOptions()
        {
            _options = new Emby.Naming.Common.NamingOptions();
            EpisodeExpzressions = _options.EpisodeExpressions
                .Select((e) => new EpisodeExpression(e.Expression, e.IsByDate)).ToArray();
            _multipleEpisodeExpressions = _options.MultipleEpisodeExpressions
                .Select((e) => new EpisodeExpression(e.Expression, e.IsByDate)).ToArray();
            VideoExtraRules = _options.VideoExtraRules
                .Select((e) => new ExtraRule((ExtraType)e.ExtraType, (ExtraRuleType)e.RuleType, e.Token, (MediaType)e.MediaType)).ToArray();
        }

        /// <summary>
        /// Gets or sets list of video file extensions.
        /// </summary>
        public string[] VideoFileExtensions => _options.VideoFileExtensions;

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
        public EpisodeExpression[] EpisodeExpzressions { get; }

        /// <summary>
        /// Gets or sets list of multi-episode regular expressions.
        /// </summary>
        public EpisodeExpression[] MultipleEpisodeExpressions => _multipleEpisodeExpressions;

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
        public ExtraRule[] VideoExtraRules { get; }
    }
}
