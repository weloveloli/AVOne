// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Official
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Impl.Constants;
    using AVOne.IO;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Models.Result;
    using AVOne.Providers;

    /// <summary>
    /// 从给定的文件路径中提取番号和相关信息
    /// </summary>
    /// <seealso cref="ILocalMetadataProvider" />
    public class OfficialLocalMetadataProvider : ILocalMetadataProvider<PornMovie>
    {
        private readonly Regex ignore_pattern;

        public int Order => 1;

        public string Name => OfficialProviderNames.Official;

        public OfficialLocalMetadataProvider(BaseApplicationConfiguration cfg)
        {
            var words = cfg.MovieID.ignore_whole_word.Replace(" ", "").Split(';');
            var regexes = cfg.MovieID.ignore_regex.Split(';');

            var wordsPattern = @"(?:\b|_)(" + string.Join("|", words) + @")(?=\b|_)";
            var regexPatterns = string.Join("|", regexes.Select(i => "(" + i + ")"));
            var patternStr = "(" + wordsPattern + ")|" + regexPatterns;

            ignore_pattern = new Regex(patternStr, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        public PornMovieInfo Parse(string filepath)
        {
            var id = GetId(filepath, out var category, out var flags);
            return string.IsNullOrEmpty(id)
                ? PornMovieInfo.Empty(filepath)
                : new PornMovieInfo
                {
                    FileName = filepath,
                    Id = id,
                    Category = category,
                    Flags = flags,
                };
        }

        public string GetId(string filepath, out MovieIdCategory category, out PornMovieFlags flags)
        {
            var filename = Path.GetFileName(filepath);
            filename = ignore_pattern.Replace(filename, string.Empty);
            category = MovieIdCategory.None;
            flags = ReosolveFlagsByName(filename);
            var filename_lc = filename.ToLower();
            Match match;
            if (filename_lc.Contains("fc2"))
            {
                // 根据FC2 Club的影片数据，FC2编号为5-7个数字
                match = Regex.Match(filename, @"fc2[^a-z\d]{0,5}(ppv[^a-z\d]{0,5})?(\d{5,7})", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    category = MovieIdCategory.Amateur;
                    flags |= PornMovieFlags.Uncensored;
                    return "FC2-" + match.Groups[2].Value;
                }
            }
            else if (filename_lc.Contains("heydouga"))
            {
                match = Regex.Match(filename, @"(heydouga)[-_]*(\d{4})[-_]0?(\d{3,5})", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    category = MovieIdCategory.Uncensor;
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
                    var avid = GetId(no_domain, out category, out flags);
                    if (!string.IsNullOrEmpty(avid))
                    {
                        return avid;
                    }
                }
                // 匹配缩写成hey的heydouga影片。由于番号分三部分，要先于后面分两部分的进行匹配
                match = Regex.Match(filename, @"(?:hey)[-_]*(\d{4})[-_]0?(\d{3,5})", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    category = MovieIdCategory.Uncensor;
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
                    category = MovieIdCategory.Uncensor;
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
                category = MovieIdCategory.Uncensor;
                flags |= PornMovieFlags.Uncensored;
                return match.Groups[1].Value;
            }

            // Try to match pure numeric IDs (uncensored videos)
            match = Regex.Match(filename, @"(\d{6}[-_]\d{2,3})");
            if (match.Success)
            {
                category = MovieIdCategory.Uncensor;
                flags |= PornMovieFlags.Uncensored;
                return match.Groups[1].Value;
            }

            // If still not match, try to replace ')(' with '-' and match again, some video IDs are separated by ')('
            if (filepath.Contains(")("))
            {
                var avid = GetId(filepath.Replace(")(", "-"), out category, out flags);
                if (!string.IsNullOrEmpty(avid))
                {
                    return avid;
                }
            }

            // If still can't match the ID, try using the name of the folder containing the file to match
            if (File.Exists(filepath))
            {
                var norm = Path.GetFullPath(filepath);
                var folder = Path.GetDirectoryName(norm)?.Split(Path.DirectorySeparatorChar)[^2];
                return string.IsNullOrEmpty(folder) ? string.Empty : GetId(folder, out category, out flags);
            }
            return string.Empty;
        }

        private static PornMovieFlags ReosolveFlagsByName(string movieName)
        {
            var flags = PornMovieFlags.None;
            if (movieName.EndsWith("-C") || movieName.Contains("中文"))
            {
                flags |= PornMovieFlags.ChineaseSubtilte;
            }
            return flags;
        }

        public Task<MetadataResult<PornMovie>> GetMetadata(ItemInfo info, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var result = new MetadataResult<PornMovie>()
                {

                };
                return result;
            });
        }
    }
}
