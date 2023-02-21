// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Resolvers
{
    using System.Diagnostics.CodeAnalysis;
    using AVOne.Enum;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Providers;
    using AVOne.Resolvers;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Resolves a Path into a Video or Video subclass.
    /// </summary>
    public class ExtraResolver
    {
        private readonly ILogger _logger;
        private readonly IProviderManager _providerManager;
        private INamingOptions _namingOptions => _providerManager.GetNamingOptionProvider().GetNamingOption();
        private IItemResolver[] _trailerResolvers => Array.Empty<IItemResolver>();
        private IItemResolver[] _videoResolvers => new IItemResolver[] { new GenericVideoResolver<Video>(_logger, _providerManager) };


        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraResolver"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="namingOptions">An instance of <see cref="NamingOptions"/>.</param>
        public ExtraResolver(ILogger logger, IProviderManager providerManager)
        {
            _logger = logger;
            _providerManager = providerManager;
        }

        /// <summary>
        /// Gets the resolvers for the extra type.
        /// </summary>
        /// <param name="extraType">The extra type.</param>
        /// <returns>The resolvers for the extra type.</returns>
        public IItemResolver[]? GetResolversForExtraType(ExtraType extraType) => extraType switch
        {
            ExtraType.Trailer => _trailerResolvers,
            // For audio we'll have to rely on the AudioResolver, which is a "built-in"
            ExtraType.ThemeSong => null,
            _ => _videoResolvers
        };

        public bool TryGetExtraTypeForOwner(string path, VideoFileInfo ownerVideoFileInfo, [NotNullWhen(true)] out ExtraType? extraType)
        {
            var extraResult = ExtraRuleResolver.GetExtraInfo(path, _namingOptions);
            if (extraResult.ExtraType == null)
            {
                extraType = null;
                return false;
            }

            var cleanDateTimeResult = CleanDateTimeParser.Clean(Path.GetFileNameWithoutExtension(path), _namingOptions.CleanDateTimeRegexes);
            var name = cleanDateTimeResult.Name;
            var year = cleanDateTimeResult.Year;

            var parentDir = ownerVideoFileInfo.IsDirectory ? ownerVideoFileInfo.Path : Path.GetDirectoryName(ownerVideoFileInfo.Path.AsSpan());

            var trimmedFileNameWithoutExtension = TrimFilenameDelimiters(ownerVideoFileInfo.FileNameWithoutExtension, _namingOptions.VideoFlagDelimiters);
            var trimmedVideoInfoName = TrimFilenameDelimiters(ownerVideoFileInfo.Name, _namingOptions.VideoFlagDelimiters);
            var trimmedExtraFileName = TrimFilenameDelimiters(name, _namingOptions.VideoFlagDelimiters);

            // first check filenames
            var isValid = StartsWith(trimmedExtraFileName, trimmedFileNameWithoutExtension)
                           || StartsWith(trimmedExtraFileName, trimmedVideoInfoName) && year == ownerVideoFileInfo.Year;

            if (!isValid)
            {
                // When the extra rule type is DirectoryName we must go one level higher to get the "real" dir name
                var currentParentDir = extraResult.Rule?.RuleType == ExtraRuleType.DirectoryName
                    ? Path.GetDirectoryName(Path.GetDirectoryName(path.AsSpan()))
                    : Path.GetDirectoryName(path.AsSpan());

                isValid = !currentParentDir.IsEmpty && !parentDir.IsEmpty && currentParentDir.Equals(parentDir, StringComparison.OrdinalIgnoreCase);
            }

            extraType = extraResult.ExtraType;
            return isValid;
        }

        private static ReadOnlySpan<char> TrimFilenameDelimiters(ReadOnlySpan<char> name, ReadOnlySpan<char> videoFlagDelimiters)
        {
            return name.IsEmpty ? name : name.TrimEnd().TrimEnd(videoFlagDelimiters).TrimEnd();
        }

        private static bool StartsWith(ReadOnlySpan<char> fileName, ReadOnlySpan<char> baseName)
        {
            return !baseName.IsEmpty && fileName.StartsWith(baseName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
