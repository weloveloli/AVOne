﻿namespace AVOne.Impl.Resolvers
{
    using AVOne.IO;
    using AVOne.Providers;

    /// <summary>
    /// Resolve <see cref="FileStack"/> from list of paths.
    /// </summary>
    public static class StackResolver
    {
        /// <summary>
        /// Resolves only directories from paths.
        /// </summary>
        /// <param name="files">List of paths.</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <returns>Enumerable <see cref="FileStack"/> of directories.</returns>
        public static IEnumerable<FileStack> ResolveDirectories(IVideoResolverProvider videoResolver, IEnumerable<string> files, INamingOptions namingOptions)
        {
            return Resolve(videoResolver,files.Select(i => new FileSystemMetadata { FullName = i, IsDirectory = true }), namingOptions);
        }

        /// <summary>
        /// Resolves only files from paths.
        /// </summary>
        /// <param name="files">List of paths.</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <returns>Enumerable <see cref="FileStack"/> of files.</returns>
        public static IEnumerable<FileStack> ResolveFiles(IVideoResolverProvider videoResolver, IEnumerable<string> files, INamingOptions namingOptions)
        {
            return Resolve(videoResolver, files.Select(i => new FileSystemMetadata { FullName = i, IsDirectory = false }), namingOptions);
        }

        /// <summary>
        /// Resolves videos from paths.
        /// </summary>
        /// <param name="files">List of paths.</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <returns>Enumerable <see cref="FileStack"/> of videos.</returns>
        public static IEnumerable<FileStack> Resolve(IVideoResolverProvider videoResolver, IEnumerable<FileSystemMetadata> files, INamingOptions namingOptions)
        {
            var potentialFiles = files
                .Where(i => i.IsDirectory || videoResolver.IsVideoFile(i.FullName, namingOptions))
                .OrderBy(i => i.FullName);

            var potentialStacks = new Dictionary<string, StackMetadata>();
            foreach (var file in potentialFiles)
            {
                var name = file.Name;
                if (string.IsNullOrEmpty(name))
                {
                    name = Path.GetFileName(file.FullName);
                }

                for (var i = 0; i < namingOptions.VideoFileStackingRules.Length; i++)
                {
                    var rule = namingOptions.VideoFileStackingRules[i];
                    if (!rule.Match(name, out var stackParsingResult))
                    {
                        continue;
                    }

                    var stackName = stackParsingResult.Value.StackName;
                    var partNumber = stackParsingResult.Value.PartNumber;
                    var partType = stackParsingResult.Value.PartType;

                    if (!potentialStacks.TryGetValue(stackName, out var stackResult))
                    {
                        stackResult = new StackMetadata(file.IsDirectory, rule.IsNumerical, partType);
                        potentialStacks[stackName] = stackResult;
                    }

                    if (stackResult.Parts.Count > 0)
                    {
                        if (stackResult.IsDirectory != file.IsDirectory
                            || !string.Equals(partType, stackResult.PartType, StringComparison.OrdinalIgnoreCase)
                            || stackResult.ContainsPart(partNumber))
                        {
                            continue;
                        }

                        if (rule.IsNumerical != stackResult.IsNumerical)
                        {
                            break;
                        }
                    }

                    stackResult.Parts.Add(partNumber, file);
                    break;
                }
            }

            foreach (var (fileName, stack) in potentialStacks)
            {
                if (stack.Parts.Count < 2)
                {
                    continue;
                }

                yield return new FileStack(fileName, stack.IsDirectory, stack.Parts.Select(kv => kv.Value.FullName).ToArray());
            }
        }

        private class StackMetadata
        {
            public StackMetadata(bool isDirectory, bool isNumerical, string partType)
            {
                Parts = new Dictionary<string, FileSystemMetadata>(StringComparer.OrdinalIgnoreCase);
                IsDirectory = isDirectory;
                IsNumerical = isNumerical;
                PartType = partType;
            }

            public Dictionary<string, FileSystemMetadata> Parts { get; }

            public bool IsDirectory { get; }

            public bool IsNumerical { get; }

            public string PartType { get; }

            public bool ContainsPart(string partNumber) => Parts.ContainsKey(partNumber);
        }
    }
}
