﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Resolvers
{
    using System;
    using System.Linq;
    using DotNet.Globbing;
    /// <summary>
    /// Glob patterns for files to ignore.
    /// </summary>
    public static class IgnorePatterns
    {
        /// <summary>
        /// Files matching these glob patterns will be ignored.
        /// </summary>
        private static readonly string[] _patterns =
        {
            "**/small.jpg",
            "**/albumart.jpg",

            // We have neither non-greedy matching or character group repetitions, working around that here.
            // https://github.com/dazinator/DotNet.Glob#patterns
            // .*/sample\..{1,5}
            "**/sample.?",
            "**/sample.??",
            "**/sample.???", // Matches sample.mkv
            "**/sample.????", // Matches sample.webm
            "**/sample.?????",
            "**/*.sample.?",
            "**/*.sample.??",
            "**/*.sample.???",
            "**/*.sample.????",
            "**/*.sample.?????",
            "**/sample/*",

            // Directories
            "**/metadata/**",
            "**/metadata",
            "**/ps3_update/**",
            "**/ps3_update",
            "**/ps3_vprm/**",
            "**/ps3_vprm",
            "**/extrafanart/**",
            "**/extrafanart",
            "**/extrathumbs/**",
            "**/extrathumbs",
            "**/.actors/**",
            "**/.actors",
            "**/.wd_tv/**",
            "**/.wd_tv",
            "**/lost+found/**",
            "**/lost+found",

            // WMC temp recording directories that will constantly be written to
            "**/TempRec/**",
            "**/TempRec",
            "**/TempSBE/**",
            "**/TempSBE",

            // Synology
            "**/eaDir/**",
            "**/eaDir",
            "**/@eaDir/**",
            "**/@eaDir",
            "**/#recycle/**",
            "**/#recycle",

            // Qnap
            "**/@Recycle/**",
            "**/@Recycle",
            "**/.@__thumb/**",
            "**/.@__thumb",
            "**/$RECYCLE.BIN/**",
            "**/$RECYCLE.BIN",
            "**/System Volume Information/**",
            "**/System Volume Information",
            "**/.grab/**",
            "**/.grab",

            // Unix hidden files
            "**/.*",

            // Mac - if you ever remove the above.
            // "**/._*",
            // "**/.DS_Store",

            // thumbs.db
            "**/thumbs.db",

            // bts sync files
            "**/*.bts",
            "**/*.sync",
        };

        private static readonly GlobOptions _globOptions = new GlobOptions
        {
            Evaluation =
            {
                CaseInsensitive = true
            }
        };

        private static readonly Glob[] _globs = _patterns.Select(p => Glob.Parse(p, _globOptions)).ToArray();

        /// <summary>
        /// Returns true if the supplied path should be ignored.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>Whether to ignore the path.</returns>
        public static bool ShouldIgnore(ReadOnlySpan<char> path)
        {
            var len = _globs.Length;
            for (var i = 0; i < len; i++)
            {
                if (_globs[i].IsMatch(path))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
