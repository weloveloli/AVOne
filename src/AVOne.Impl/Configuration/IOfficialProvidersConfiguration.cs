// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable
namespace AVOne.Configuration
{
    using AVOne.Impl.Helper;

    public interface IOfficialProvidersConfiguration
    {
        public MetaTubeConfiguration MetaTube { get; }
    }

    public class MetaTubeConfiguration
    {
        public string Server { get; set; } = "localhost";
        public int DefaultImageQuality { get; set; } = -1;
        public double PrimaryImageRatio { get; set; } = 90;
        public string DefaultUserAgent => $"AVOne";
        public string Token { get; set; } = string.Empty;
        public bool EnableCollections { get; set; } = false;

        public bool EnableDirectors { get; set; } = true;

        public bool EnableRatings { get; set; } = true;

        public bool EnableTrailers { get; set; } = false;

        public bool EnableRealActorNames { get; set; } = false;

        public bool EnableMovieProviderFilter { get; set; } = false;

        public string RawMovieProviderFilter
        {
            get => _movieProviderFilter?.Any() == true ? string.Join(',', _movieProviderFilter) : string.Empty;
            set => _movieProviderFilter = value?.Split(',').Select(s => s.Trim()).Where(s => s.Any())
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public List<string> GetMovieProviderFilter()
        {
            return _movieProviderFilter;
        }

        private List<string> _movieProviderFilter;

        public bool EnableTitleSubstitution { get; set; } = false;

        public string TitleRawSubstitutionTable
        {
            get => _titleSubstitutionTable?.ToString();
            set => _titleSubstitutionTable = SubstitutionTable.Parse(value);
        }

        public SubstitutionTable GetTitleSubstitutionTable()
        {
            return _titleSubstitutionTable;
        }

        private SubstitutionTable _titleSubstitutionTable;

        public bool EnableActorSubstitution { get; set; } = false;

        public string ActorRawSubstitutionTable
        {
            get => _actorSubstitutionTable?.ToString();
            set => _actorSubstitutionTable = SubstitutionTable.Parse(value);
        }

        public SubstitutionTable GetActorSubstitutionTable()
        {
            return _actorSubstitutionTable;
        }

        private SubstitutionTable _actorSubstitutionTable;

        public bool EnableGenreSubstitution { get; set; } = false;

        public string GenreRawSubstitutionTable
        {
            get => _genreSubstitutionTable?.ToString();
            set => _genreSubstitutionTable = SubstitutionTable.Parse(value);
        }

        public SubstitutionTable GetGenreSubstitutionTable()
        {
            return _genreSubstitutionTable;
        }

        private SubstitutionTable _genreSubstitutionTable;

        #region Template

        public bool EnableTemplate { get; set; } = false;

        public string NameTemplate { get; set; } = DefaultNameTemplate;

        public string TaglineTemplate { get; set; } = DefaultTaglineTemplate;

        public static string DefaultNameTemplate => "{number} {title}";

        public static string DefaultTaglineTemplate => "配信開始日 {date}";

        #endregion
    }
}
