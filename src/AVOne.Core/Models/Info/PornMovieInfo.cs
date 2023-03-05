// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Models.Info
{
    using System.IO;
    using System.Text.RegularExpressions;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Models.Item;

    public class PornMovieInfo : ItemLookupInfo
    {
        /// <summary>
        /// 类型
        /// </summary>
        public AVCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the flags.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public PornMovieFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Id"/> is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if valid; otherwise, <c>false</c>.
        /// </value>
        public bool Valid { get; set; } = true;

        /// <summary>
        /// 解析到的id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Id;

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="id"></param>
        public static implicit operator PornMovieInfo(string id)
            => new() { Id = id };

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="id"></param>
        public static implicit operator string(PornMovieInfo id)
            => id?.Id;

        public static PornMovieInfo Empty(PornMovie movie) => new() { Valid = false, Category = AVCategory.None, Id = string.Empty, Name = movie.Name, Path = movie.Path, Flags = PornMovieFlags.None };

        public static PornMovieInfo Parse(PornMovie movie, BaseApplicationConfiguration cfg)
        {
            var filepath = string.IsNullOrEmpty(movie.Path) ? movie.Name : movie.Path;

            if (string.IsNullOrEmpty(filepath))
            {
                return Empty(movie);
            }

            var words = cfg.MovieID.ignore_whole_word.Replace(" ", "").Split(';');
            var regexes = cfg.MovieID.ignore_regex.Split(';');

            var wordsPattern = @"(?:\b|_)(" + string.Join("|", words) + @")(?=\b|_)";
            var regexPatterns = string.Join("|", regexes.Select(i => "(" + i + ")"));
            var patternStr = "(" + wordsPattern + ")|" + regexPatterns;

            var ignore_pattern = new Regex(patternStr, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var id = GetId(ignore_pattern, filepath, out var category, out var flags);
            return string.IsNullOrEmpty(id)
                ? Empty(movie)
                : new PornMovieInfo
                {
                    FileName = filepath,
                    Id = id,
                    Category = category,
                    Flags = flags,
                    Name = movie.Name,
                    Path = movie.Path
                };
        }

        public static string GetId(Regex ignore_pattern, string filepath, out AVCategory category, out PornMovieFlags flags)
        {
            var filename = System.IO.Path.GetFileName(filepath);
            if (ignore_pattern is not null)
            {
                filename = ignore_pattern.Replace(filename, string.Empty);
            }

            category = AVCategory.None;
            var filename_lc = filename.ToLower();
            flags = ReosolveFlagsByName(filename_lc);
            Match match;
            if (filename_lc.Contains("fc2"))
            {
                // 根据FC2 Club的影片数据，FC2编号为5-7个数字
                match = Regex.Match(filename, @"fc2[^a-z\d]{0,5}(ppv[^a-z\d]{0,5})?(\d{5,7})", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    category = AVCategory.Amateur;
                    flags |= PornMovieFlags.Uncensored;
                    return "FC2-" + match.Groups[2].Value;
                }
            }
            else if (filename_lc.Contains("heydouga"))
            {
                match = Regex.Match(filename, @"(heydouga)[-_]*(\d{4})[-_]0?(\d{3,5})", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    category = AVCategory.Censored;
                    flags |= PornMovieFlags.Uncensored;
                    return string.Join("-", match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
                }
            }
            else
            {
                // 先尝试移除可疑域名进行匹配，如果匹配不到再使用原始文件名进行匹配
                var no_domain = Regex.Replace(filename, @"\w{3,10}\.(com|net|app|xyz)", string.Empty, RegexOptions.IgnoreCase);
                if (no_domain != filename)
                {
                    var avid = GetId(ignore_pattern, no_domain, out category, out flags);
                    if (!string.IsNullOrEmpty(avid))
                    {
                        return avid;
                    }
                }
                // 匹配缩写成hey的heydouga影片。由于番号分三部分，要先于后面分两部分的进行匹配
                match = Regex.Match(filename, @"(?:hey)[-_]*(\d{4})[-_]0?(\d{3,5})", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    category = AVCategory.Uncensored;
                    flags |= PornMovieFlags.Uncensored;
                    return "heydouga-" + string.Join("-", match.Groups[1].Value, match.Groups[2].Value);
                }
                // 普通番号，优先尝试匹配带分隔符的（如ABC-123）
                match = Regex.Match(filename, @"([a-z]{2,10})[-_](\d{2,5})", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value + "-" + match.Groups[2].Value;
                }
                // 普通番号，运行到这里时表明无法匹配到带分隔符的番号
                // 先尝试匹配东热的red, sky, ex三个不带-分隔符的系列
                match = Regex.Match(filename, @"(red[01]\d\d|sky[0-3]\d\d|ex00[01]\d)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    category = AVCategory.Uncensored;
                    flags |= PornMovieFlags.Uncensored;
                    return match.Groups[1].Value;
                }
                // 然后再将影片视作缺失了-分隔符来匹配
                match = Regex.Match(filename, @"([a-z]{2,})(\d{2,5})", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value + '-' + match.Groups[2].Value;
                }
                // Try to match TMA produced videos (e.g. 'T28-557', their IDs are messy)
                match = Regex.Match(filename, @"(T28[-_]\d{3})");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            // Try to match Tokyo-hot n, k series
            match = Regex.Match(filename, @"(n\d{4}|k\d{4})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                category = AVCategory.Uncensored;
                flags |= PornMovieFlags.Uncensored;
                return match.Groups[1].Value;
            }

            // Try to match pure numeric IDs (uncensored videos)
            match = Regex.Match(filename, @"(\d{6}[-_]\d{2,3})");
            if (match.Success)
            {
                category = AVCategory.Uncensored;
                flags |= PornMovieFlags.Uncensored;
                return match.Groups[1].Value;
            }

            // If still not match, try to replace ')(' with '-' and match again, some video IDs are separated by ')('
            if (filepath.Contains(")("))
            {
                var avid = GetId(ignore_pattern, filepath.Replace(")(", "-"), out category, out flags);
                if (!string.IsNullOrEmpty(avid))
                {
                    return avid;
                }
            }

            // If still can't match the ID, try using the name of the folder containing the file to match
            if (File.Exists(filepath))
            {
                var norm = System.IO.Path.GetFullPath(filepath);
                var folder = System.IO.Path.GetDirectoryName(norm)?.Split(System.IO.Path.DirectorySeparatorChar)[^2];
                return string.IsNullOrEmpty(folder) ? string.Empty : GetId(ignore_pattern, folder, out category, out flags);
            }
            return string.Empty;
        }

        private static PornMovieFlags ReosolveFlagsByName(string movieName)
        {
            var flags = PornMovieFlags.None;
            if (string.IsNullOrEmpty(movieName))
            {
                return flags;
            }

            if (movieName.Contains("-c") || movieName.Contains("-ch") || movieName.Contains("chinese") || movieName.Contains("中文"))
            {
                flags |= PornMovieFlags.ChineseSubtilte;
            }
            return flags;
        }
    }
}
