// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Official
{
    using System.Text.RegularExpressions;
    using AVOne.Enum;
    using AVOne.Extensions;
    using AVOne.Impl.Constants;
    using AVOne.Providers;

    public class OfficialMovieNameParserProvider : IPornMovieNameParserProvider
    {
        public int Order => -1;

        public string Name => OfficialProviderNames.Official;

        public MovieId Parse(string movieName)
        {
            movieName = movieName.Replace("_", "-").Replace(" ", "-").Replace(".", "-");

            var m = p1080p.Match(movieName);
            while (m.Success)
            {
                movieName = movieName.Replace(m.Groups["p"].Value, "");
                m = m.NextMatch();
            }

            foreach (var func in funcs)
            {
                var r = func(movieName);
                if (r != null)
                    return r;
            }

            movieName = Regex.Replace(movieName, @"ts6[\d]+", "", options);
            movieName = Regex.Replace(movieName, @"-*whole\d*", "", options);
            movieName = Regex.Replace(movieName, @"-*full$", "", options);
            movieName = movieName.Replace("tokyo-hot", "", StringComparison.OrdinalIgnoreCase);
            movieName = movieName.TrimEnd("-C").TrimEnd("-HD", "-full", "full").TrimStart("HD-").TrimStart("h-");
            movieName = Regex.Replace(movieName, @"\d{2,4}-\d{1,2}-\d{1,2}", "", options); //日期
            movieName = Regex.Replace(movieName, @"(.*)(00)(\d{3})", "$1-$3", options); //FANZA cid AAA00111
            //标准 AAA-111
            m = Regex.Match(movieName, @"(^|[^a-z0-9])(?<id>[a-z0-9]{2,10}-[\d]{2,8})($|[^\d])", options);
            if (m.Success && m.Groups["id"].Value.Length >= 4)
                return m.Groups["id"].Value;
            //第二段带字母 AAA-B11
            m = Regex.Match(movieName, @"(^|[^a-z0-9])(?<id>[a-z]{2,10}-[a-z]{1,5}[\d]{2,8})($|[^\d])", options);
            if (m.Success && m.Groups["id"].Value.Length >= 4)
                return m.Groups["id"].Value;
            //没有横杠的 AAA111
            m = Regex.Match(movieName, @"(^|[^a-z0-9])(?<id>[a-z]{1,10}[\d]{2,8})($|[^\d])", options);
            if (m.Success && m.Groups["id"].Value.Length >= 4)
                return m.Groups["id"].Value;

            return null;
        }

        private static RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled;

        private static Func<string, MovieId>[] funcs = new Func<string, MovieId>[] {
            Carib,
            Heyzo,
            FC2,
            Musume,
            OnlyNumber
        };

        /// <summary>
        /// 移除视频编码 1080p,720p 2k 之类的
        /// </summary>
        private static Regex p1080p = new Regex(@"(^|[^\d])(?<p>[\d]{3,5}p|[\d]{1,2}k)($|[^a-z])", options);

        private static Regex[] regexMusume = new Regex[] {
            new Regex(@"(?<id>[\d]{4,8}-[\d]{1,6})-(10mu)",options),
            new Regex(@"(10Musume)-(?<id>[\d]{4,8}-[\d]{1,6})",options)
        };

        private static MovieId Musume(string name)
        {
            foreach (var regex in regexMusume)
            {
                var m = regex.Match(name);
                if (m.Success)
                    return new MovieId()
                    {
                        Matcher = nameof(Musume),
                        Type = MovieIdCategory.suren,
                        Id = m.Groups["id"].Value.Replace("_", "-")
                    };
            }
            return null;
        }

        private static Regex[] regexCarib = new Regex[] {
            new Regex(@"(?<id>[\d]{4,8}-[\d]{1,6})-(1pon|carib|paco|mura)",options),
            new Regex(@"(1Pondo|Caribbean|Pacopacomama|muramura)-(?<id>[\d]{4,8}-[\d]{1,8})($|[^\d])",options)
        };

        private static MovieId Carib(string name)
        {
            foreach (var regex in regexCarib)
            {
                var m = regex.Match(name);
                if (m.Success)
                    return new MovieId()
                    {
                        Matcher = nameof(Carib),
                        Type = MovieIdCategory.Uncensor,
                        Id = m.Groups["id"].Value.Replace("-", "_")
                    };
            }
            return null;
        }

        private static Regex regexHeyzo = new Regex(@"Heyzo(|-| |.com)(HD-|)(?<id>[\d]{2,8})($|[^\d])", options);

        private static MovieId Heyzo(string name)
        {
            var m = regexHeyzo.Match(name);
            if (m.Success == false)
                return null;
            var id = $"HEYZO-{m.Groups["id"]}";
            return new MovieId()
            {
                Matcher = nameof(Heyzo),
                Id = id,
                Type = MovieIdCategory.Uncensor
            };
        }

        private static Regex regexFC2 = new Regex(@"FC2-*(PPV|)[^\d]{1,3}(?<id>[\d]{2,10})($|[^\d])", options);

        public static MovieId FC2(string name)
        {
            var m = regexFC2.Match(name);
            if (m.Success == false)
                return null;
            var id = $"FC2-{m.Groups["id"]}";
            return new MovieId()
            {
                Id = id,
                Matcher = nameof(FC2),
                Type = MovieIdCategory.Amateur
            };
        }

        private static Regex regexNumber = new Regex(@"(?<id>[\d]{6,8}-[\d]{1,6})", options);

        private static MovieId OnlyNumber(string name)
        {
            var m = regexNumber.Match(name);
            if (m.Success == false)
                return null;
            var id = m.Groups["id"].Value;
            return new MovieId()
            {
                Matcher = nameof(OnlyNumber),
                Id = id
            };
        }
    }
}
