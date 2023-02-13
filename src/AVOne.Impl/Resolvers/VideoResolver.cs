// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Resolvers
{
    using System.Diagnostics.CodeAnalysis;
    using AVOne.Impl.Helper;
    using AVOne.Models.Info;
    using AVOne.Providers;
    using AVOne.Extensions;

    /// <summary>
    /// Resolves <see cref="VideoFileInfo"/> from file path.
    /// </summary>
    public static class VideoResolver
    {
        /// <summary>
        /// Resolves the directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <param name="parseName">Whether to parse the name or use the filename.</param>
        /// <returns>VideoFileInfo.</returns>
        public static VideoFileInfo? ResolveDirectory(string? path, INamingOptions namingOptions, bool parseName = true)
        {
            return Resolve(path, true, namingOptions, parseName);
        }

        /// <summary>
        /// Resolves the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <returns>VideoFileInfo.</returns>
        public static VideoFileInfo? ResolveFile(string? path, INamingOptions namingOptions)
        {
            return Resolve(path, false, namingOptions);
        }

        /// <summary>
        /// Resolves the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="isDirectory">if set to <c>true</c> [is folder].</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <param name="parseName">Whether or not the name should be parsed for info.</param>
        /// <returns>VideoFileInfo.</returns>
        /// <exception cref="ArgumentNullException"><c>path</c> is <c>null</c>.</exception>
        public static VideoFileInfo? Resolve(string? path, bool isDirectory, INamingOptions namingOptions, bool parseName = true)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            bool isStub = false;
            ReadOnlySpan<char> container = ReadOnlySpan<char>.Empty;
            string? stubType = null;

            if (!isDirectory)
            {
                var extension = Path.GetExtension(path.AsSpan());

                // Check supported extensions
                if (!namingOptions.VideoFileExtensions.Contains(extension, StringComparison.OrdinalIgnoreCase))
                {
                    // It's not supported. Check stub extensions
                    if (!StubResolver.TryResolveFile(path, namingOptions, out stubType))
                    {
                        return null;
                    }

                    isStub = true;
                }

                container = extension.TrimStart('.');
            }

            var extraResult = ExtraRuleResolver.GetExtraInfo(path, namingOptions);

            var name = Path.GetFileNameWithoutExtension(path);

            int? year = null;

            if (parseName)
            {
                var cleanDateTimeResult = CleanDateTime(name, namingOptions);
                name = cleanDateTimeResult.Name;
                year = cleanDateTimeResult.Year;

                if (extraResult.ExtraType == null
                    && TryCleanString(name, namingOptions, out var newName))
                {
                    name = newName;
                }
            }

            return new VideoFileInfo(
                path: path,
                container: container.IsEmpty ? null : container.ToString(),
                isStub: isStub,
                name: name,
                year: year,
                stubType: stubType,
                extraType: extraResult.ExtraType,
                isDirectory: isDirectory,
                extraRule: extraResult.Rule);
        }

        /// <summary>
        /// Determines if path is video file based on extension.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <returns>True if is video file.</returns>
        public static bool IsVideoFile(string path, INamingOptions namingOptions)
        {
            var extension = Path.GetExtension(path.AsSpan());
            return namingOptions.VideoFileExtensions.Contains(extension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if path is video file stub based on extension.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <returns>True if is video file stub.</returns>
        public static bool IsStubFile(string path, INamingOptions namingOptions)
        {
            var extension = Path.GetExtension(path.AsSpan());
            return namingOptions.StubFileExtensions.Contains(extension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Tries to clean name of clutter.
        /// </summary>
        /// <param name="name">Raw name.</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <param name="newName">Clean name.</param>
        /// <returns>True if cleaning of name was successful.</returns>
        public static bool TryCleanString([NotNullWhen(true)] string? name, INamingOptions namingOptions, out string newName)
        {
            return CleanStringParser.TryClean(name, namingOptions.CleanStringRegexes, out newName);
        }

        /// <summary>
        /// Tries to get name and year from raw name.
        /// </summary>
        /// <param name="name">Raw name.</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <returns>Returns <see cref="CleanDateTimeResult"/> with name and optional year.</returns>
        public static CleanDateTimeResult CleanDateTime(string name, INamingOptions namingOptions)
        {
            return CleanDateTimeParser.Clean(name, namingOptions.CleanDateTimeRegexes);
        }
    }
}
