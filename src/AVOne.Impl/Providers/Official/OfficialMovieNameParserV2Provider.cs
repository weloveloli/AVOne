// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Official
{
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using AVOne.Configuration;
    using AVOne.Enum;
    using AVOne.Impl.Constants;
    using AVOne.Providers;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// 从给定的文件路径中提取番号
    /// </summary>
    /// <seealso cref="AVOne.Providers.IPornMovieNameParserProvider" />
    public class OfficialMovieNameParserV2Provider : IPornMovieNameParserProvider
    {
        private readonly IApplicationConfigs _configs;

        public int Order => 1;

        public string Name => OfficialProviderNames.Official;

        public OfficialMovieNameParserV2Provider(IApplicationConfigs configs)
        {
            this._configs = configs;
        }

        public MovieId? Parse(string movieName)
        {
            if (string.IsNullOrWhiteSpace(movieName))
            {
                throw new ArgumentException($"'{nameof(movieName)}' cannot be null or whitespace.", nameof(movieName));
            }

            var partterns = _configs.IgnorePatterns;
            if(partterns?.Any() ?? false) 
            {
                foreach(var part in partterns)                                                                                                                                                  
                {
                    movieName = movieName.Replace(part, "");
                }
            }
            movieName = movieName.ToLower();

            foreach(var fun in funcs)
            {
                var movieId = fun(movieName);
                if (movieId is not null)
                {
                    return movieId;
                }
            }
            return null;
        }
        private static RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled;
        private static Func<string, MovieId?>[] funcs = new Func<string, MovieId?>[] {
            FC2
        };

        private static MovieId? FC2(string movieName)
        {
            if (movieName.Contains("fc2"))
            {
                var regexFC2 = new Regex(@"FC2-*(PPV|)[^\d]{1,3}(?<id>[\d]{2,10})($|[^\d])", options);
                var m = regexFC2.Match(movieName);
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
            return null;
        }

    }
}
