﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Providers.Metatube
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AVOne.Constants;
    using AVOne.Extensions;
    using AVOne.Impl.Extensions;
    using AVOne.Providers.Metatube.Models;
    using AVOne.Models.Info;
    using AVOne.Models.Item;
    using AVOne.Models.Result;
    using AVOne.Providers;
    using Microsoft.Extensions.Logging;
    using AVOne.Configuration;
    using AVOne.Providers.MetaTube.Configuration;

    public class MetatubeMovieMetaDataProvider : BaseProvider, IRemoteMetadataProvider<PornMovie, PornMovieInfo>
    {
        private const string AvWiki = "AVWIKI";
        private const string GFriends = "GFriends";
        private const string Rating = "JP-18+";

        private static readonly string[] AvWikiSupportedProviderNames = { "DUGA", "FANZA", "Getchu", "MGS", "Pcolle" };

        public MetatubeMovieMetaDataProvider(ILogger<MetatubeMovieMetaDataProvider> logger,
                                             IConfigurationManager configurationManager,
                                             MetatubeApiClient metatubeApiClient)
            : base(logger, configurationManager, metatubeApiClient)
        {
        }

        public async Task<MetadataResult<PornMovie>> GetMetadata(PornMovieInfo info, CancellationToken cancellationToken)
        {
            var pid = info.GetPid(Name);
            if (string.IsNullOrWhiteSpace(pid.Id) || string.IsNullOrWhiteSpace(pid.Provider))
            {
                // Search movies and pick the first result.
                var firstResult = (await GetSearchResults(info, cancellationToken)).FirstOrDefault();
                if (firstResult != null)
                {
                    pid = firstResult.GetPid(Name);
                }
            }
            Logger.Info("Get movie info: {0}", pid.ToString());
            var m = await ApiClient.GetMovieInfoAsync(pid.Provider, pid.Id, cancellationToken);

            // Preserve original title.
            var originalTitle = m.Title;

            // Convert to real actor names.
            if (Configuration.EnableRealActorNames)
            {
                await ConvertToRealActorNames(m, cancellationToken);
            }

            // Substitute title.
            if (Configuration.EnableTitleSubstitution)
            {
                m.Title = Configuration.GetTitleSubstitutionTable().Substitute(m.Title);
            }

            // Substitute actors.
            if (Configuration.EnableActorSubstitution)
            {
                m.Actors = Configuration.GetActorSubstitutionTable().Substitute(m.Actors).ToArray();
            }

            // Substitute genres.
            if (Configuration.EnableGenreSubstitution)
            {
                m.Genres = Configuration.GetGenreSubstitutionTable().Substitute(m.Genres).ToArray();
            }

            // Build parameters.
            var parameters = new Dictionary<string, string>
        {
            { @"{provider}", m.Provider },
            { @"{id}", m.Id },
            { @"{number}", m.Number },
            { @"{title}", m.Title },
            { @"{series}", m.Series },
            { @"{maker}", m.Maker },
            { @"{label}", m.Label },
            { @"{director}", m.Director },
            { @"{actors}", m.Actors?.Any() == true ? string.Join(' ', m.Actors) : string.Empty },
            { @"{first_actor}", m.Actors?.FirstOrDefault() ?? string.Empty },
            { @"{year}", $"{m.ReleaseDate:yyyy}" },
            { @"{month}", $"{m.ReleaseDate:MM}" },
            { @"{date}", $"{m.ReleaseDate:yyyy-MM-dd}" }
        };

            var result = new MetadataResult<PornMovie>
            {
                Item = new PornMovie
                {
                    Name = RenderTemplate(
                        Configuration.EnableTemplate
                            ? Configuration.NameTemplate
                            : MetaTubeConfiguration.DefaultNameTemplate, parameters),
                    Tagline = RenderTemplate(
                        Configuration.EnableTemplate
                            ? Configuration.TaglineTemplate
                            : MetaTubeConfiguration.DefaultTaglineTemplate, parameters),
                    OriginalTitle = originalTitle,
                    Overview = m.Summary,
                    OfficialRating = Rating,
                    PremiereDate = m.ReleaseDate.GetValidDateTime(),
                    ProductionYear = m.ReleaseDate.GetValidYear(),
                    Genres = m.Genres?.Any() == true ? m.Genres : Array.Empty<string>(),
                    Path = info.Path,
                    PornMovieInfo = info
                },
                HasMetadata = true
            };

            // Set provider id.
            result.Item.SetPid(Name, m.Provider, m.Id, pid.Position);

            // Set trailer url.
            result.Item.SetTrailerUrl(!string.IsNullOrWhiteSpace(m.PreviewVideoUrl)
                ? m.PreviewVideoUrl
                : m.PreviewVideoHlsUrl);

            // Set community rating.
            if (Configuration.EnableRatings)
            {
                result.Item.CommunityRating = m.Score > 0 ? (float)Math.Round(m.Score * 2, 1) : null;
            }

            // Add collection.
            if (Configuration.EnableCollections && !string.IsNullOrWhiteSpace(m.Series))
            {
                result.Item.CollectionName = m.Series;
            }

            // Add studio.
            if (!string.IsNullOrWhiteSpace(m.Maker))
            {
                result.Item.AddStudio(m.Maker);
            }

            // Add tag (series).
            if (!string.IsNullOrWhiteSpace(m.Series))
            {
                result.Item.AddTag(m.Series);
            }

            // Add tag (maker).
            if (!string.IsNullOrWhiteSpace(m.Maker))
            {
                result.Item.AddTag(m.Maker);
            }

            // Add tag (label).
            if (!string.IsNullOrWhiteSpace(m.Label))
            {
                result.Item.AddTag(m.Label);
            }

            // Add director.
            if (Configuration.EnableDirectors && !string.IsNullOrWhiteSpace(m.Director))
            {
                result.AddPerson(new PersonInfo
                {
                    Name = m.Director,
                    Type = PersonType.Director
                });
            }

            // Add actors.
            foreach (var name in m.Actors ?? Enumerable.Empty<string>())
            {
                result.AddPerson(new PersonInfo
                {
                    Name = name,
                    Type = PersonType.Actor,
                    ImageUrl = await GetActorImageUrl(name, cancellationToken)
                });
            }

            return result;
        }

        public async Task<IEnumerable<RemoteMetadataSearchResult>> GetSearchResults(PornMovieInfo info, CancellationToken cancellationToken)
        {
            var pid = info.GetPid(Name);

            var searchResults = new List<MovieSearchResult>();
            if (string.IsNullOrWhiteSpace(pid.Id) || string.IsNullOrWhiteSpace(pid.Provider))
            {
                if (info.Valid)
                {
                    // Search movie by id.
                    Logger.Info("Search for movie : {0} by id : {}", info.Id);
                    searchResults.AddRange(await ApiClient.SearchMovieAsync(info.Id, pid.Provider, cancellationToken));
                }
                else
                {
                    // Search movie by name.
                    Logger.Info("Search for movie: {0} by name", info.Name);
                    searchResults.AddRange(await ApiClient.SearchMovieAsync(info.Name, pid.Provider, cancellationToken));
                }

            }
            else
            {
                // Exact search.
                Logger.Info("Search for movie: {0}", pid.ToString());
                searchResults.Add(await ApiClient.GetMovieInfoAsync(pid.Provider, pid.Id,
                    pid.Update != true, cancellationToken));
            }

            var results = new List<RemoteMetadataSearchResult>();
            if (!searchResults.Any())
            {
                Logger.Warn("Movie not found: {0}", pid.Id);
                return results;
            }

            if (Configuration.EnableMovieProviderFilter)
            {
                var filter = Configuration.GetMovieProviderFilter();
                // Filter out mismatched results.
                _ = searchResults.RemoveAll(m => !filter.Contains(m.Provider, StringComparer.OrdinalIgnoreCase));
                // Reorder results by stable sort.
                searchResults = searchResults
                    .OrderBy(m => filter.FindIndex(s => s.Equals(m.Provider, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            foreach (var m in searchResults)
            {
                var result = new RemoteMetadataSearchResult
                {
                    Name = $"[{m.Provider}] {m.Number} {m.Title}",
                    SearchProviderName = Name,
                    PremiereDate = m.ReleaseDate.GetValidDateTime(),
                    ProductionYear = m.ReleaseDate.GetValidYear(),
                    ImageUrl = ApiClient.GetPrimaryImageApiUrl(m.Provider, m.Id, m.ThumbUrl, 1.0, true)
                };
                result.SetPid(Name, m.Provider, m.Id, pid.Position);
                results.Add(result);
            }

            return results;
        }

        private async Task<string> GetActorImageUrl(string name, CancellationToken cancellationToken)
        {
            try
            {
                // Use GFriends as actor image provider.
                foreach (var actor in (await ApiClient.SearchActorAsync(name, GFriends, false, cancellationToken))
                         .Where(actor => actor.Images?.Any() == true))
                {
                    return actor.Images.First();
                }
            }
            catch (Exception e)
            {
                Logger.Error("Get actor image error: {0} ({1})", name, e.Message);
            }

            return string.Empty;
        }

        private async Task ConvertToRealActorNames(MovieSearchResult m, CancellationToken cancellationToken)
        {
            if (!AvWikiSupportedProviderNames.Contains(m.Provider, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                var searchResults = await ApiClient.SearchMovieAsync(m.Id, AvWiki, cancellationToken);
                if (!searchResults.Any())
                {
                    Logger.Warn("Movie not found on AVWIKI: {0}", m.Id);
                }
                else if (searchResults.Count > 1)
                {
                    // Ignore multiple results to avoid ambiguity.
                    Logger.Warn("Multiple movies found on AVWIKI: {0}", m.Id);
                }
                else
                {
                    var firstResult = searchResults.First();
                    if (firstResult.Actors?.Any() == true)
                    {
                        m.Actors = firstResult.Actors;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Convert to real actor names error: {0} ({1})", m.Number, e.Message);
            }
        }
        private static string RenderTemplate(string template, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return string.Empty;
            }

            var sb = parameters.Where(kvp => template.Contains(kvp.Key))
                .Aggregate(new StringBuilder(template),
                    (sb, kvp) => sb.Replace(kvp.Key, kvp.Value));

            return sb.ToString().Trim();
        }
    }
}
