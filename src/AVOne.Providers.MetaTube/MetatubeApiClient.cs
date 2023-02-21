// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Providers.Metatube
{

    using System.Collections.Specialized;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Web;
    using AVOne.Configuration;
    using AVOne.Providers.Metatube.Models;
    using AVOne.Providers.MetaTube.Configuration;

    public class MetatubeApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfigurationManager _configurationManager;

        private MetaTubeConfiguration _metaTubeConfiguration => _configurationManager.GetConfiguration<MetaTubeConfiguration>(MetaTubeConfigStore.StoreKey);
        public MetatubeApiClient(HttpClient httpClient, IConfigurationManager configurationManager)
        {
            _httpClient = httpClient;
            _configurationManager = configurationManager;
        }

        private const string ActorInfoApi = "/v1/actors";
        private const string MovieInfoApi = "/v1/movies";
        private const string ActorSearchApi = "/v1/actors/search";
        private const string MovieSearchApi = "/v1/movies/search";
        private const string PrimaryImageApi = "/v1/images/primary";
        private const string ThumbImageApi = "/v1/images/thumb";
        private const string BackdropImageApi = "/v1/images/backdrop";

        private string ComposeUrl(string path, NameValueCollection nv)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (string key in nv)
            {
                query.Add(key, nv.Get(key));
            }

            // Build URL
            var uriBuilder = new UriBuilder(_metaTubeConfiguration.Server)
            {
                Path = path,
                Query = query.ToString() ?? string.Empty
            };
            return uriBuilder.ToString();
        }

        private string ComposeImageApiUrl(string path, string provider, string id, string url = default,
            double ratio = -1, double position = -1, bool auto = false, string badge = default)
        {
            return ComposeUrl(Path.Combine(path, provider, id), new NameValueCollection
        {
            { "url", url },
            { "ratio", ratio.ToString("R") },
            { "pos", position.ToString("R") },
            { "auto", auto.ToString() },
            { "badge", badge },
            { "quality", _metaTubeConfiguration.DefaultImageQuality.ToString() }
        });
        }

        private string ComposeInfoApiUrl(string path, string provider, string id, bool lazy)
        {
            return ComposeUrl(Path.Combine(path, provider, id), new NameValueCollection
        {
            { "lazy", lazy.ToString() }
        });
        }

        private string ComposeSearchApiUrl(string path, string q, string provider, bool fallback)
        {
            return ComposeUrl(path, new NameValueCollection
        {
            { "q", q },
            { "provider", provider },
            { "fallback", fallback.ToString() }
        });
        }

        public string GetPrimaryImageApiUrl(string provider, string id, double position = -1, string badge = default)
        {
            return ComposeImageApiUrl(PrimaryImageApi, provider, id,
                ratio: _metaTubeConfiguration.PrimaryImageRatio, position: position, badge: badge);
        }

        public string GetPrimaryImageApiUrl(string provider, string id, string url, double position = -1,
            bool auto = false, string badge = default)
        {
            return ComposeImageApiUrl(PrimaryImageApi, provider, id, url,
                _metaTubeConfiguration.PrimaryImageRatio, position, auto, badge);
        }

        public string GetThumbImageApiUrl(string provider, string id)
        {
            return ComposeImageApiUrl(ThumbImageApi, provider, id);
        }

        public string GetThumbImageApiUrl(string provider, string id, string url, double position = -1,
            bool auto = false)
        {
            return ComposeImageApiUrl(ThumbImageApi, provider, id, url, position: position, auto: auto);
        }

        public string GetBackdropImageApiUrl(string provider, string id)
        {
            return ComposeImageApiUrl(BackdropImageApi, provider, id);
        }

        public string GetBackdropImageApiUrl(string provider, string id, string url, double position = -1,
            bool auto = false)
        {
            return ComposeImageApiUrl(BackdropImageApi, provider, id, url, position: position, auto: auto);
        }

        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", _metaTubeConfiguration.DefaultUserAgent);
            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return response;

        }

        public async Task<ActorInfo> GetActorInfoAsync(string provider, string id,
            CancellationToken cancellationToken)
        {
            return await GetActorInfoAsync(provider, id, true /* default */, cancellationToken);
        }

        public async Task<ActorInfo> GetActorInfoAsync(string provider, string id, bool lazy,
            CancellationToken cancellationToken)
        {
            var apiUrl = ComposeInfoApiUrl(ActorInfoApi, provider, id, lazy);
            return await GetDataAsync<ActorInfo>(apiUrl, true, cancellationToken);
        }

        public async Task<MovieInfo> GetMovieInfoAsync(string provider, string id,
            CancellationToken cancellationToken)
        {
            return await GetMovieInfoAsync(provider, id, true /* default */, cancellationToken);
        }

        public async Task<MovieInfo> GetMovieInfoAsync(string provider, string id, bool lazy,
            CancellationToken cancellationToken)
        {
            var apiUrl = ComposeInfoApiUrl(MovieInfoApi, provider, id, lazy);
            return await GetDataAsync<MovieInfo>(apiUrl, true, cancellationToken);
        }

        public async Task<List<ActorSearchResult>> SearchActorAsync(string q,
            CancellationToken cancellationToken)
        {
            return await SearchActorAsync(q, string.Empty, cancellationToken);
        }

        public async Task<List<ActorSearchResult>> SearchActorAsync(string q, string provider,
            CancellationToken cancellationToken)
        {
            return await SearchActorAsync(q, provider, true /* default */, cancellationToken);
        }

        public async Task<List<ActorSearchResult>> SearchActorAsync(string q, string provider,
            bool fallback, CancellationToken cancellationToken)
        {
            var apiUrl = ComposeSearchApiUrl(ActorSearchApi, q, provider, fallback);
            return await GetDataAsync<List<ActorSearchResult>>(apiUrl, true, cancellationToken);
        }

        public async Task<List<MovieSearchResult>> SearchMovieAsync(string q,
            CancellationToken cancellationToken)
        {
            return await SearchMovieAsync(q, string.Empty, cancellationToken);
        }

        public async Task<List<MovieSearchResult>> SearchMovieAsync(string q, string provider,
            CancellationToken cancellationToken)
        {
            return await SearchMovieAsync(q, provider, true /* default */, cancellationToken);
        }

        public async Task<List<MovieSearchResult>> SearchMovieAsync(string q, string provider,
            bool fallback, CancellationToken cancellationToken)
        {
            var apiUrl = ComposeSearchApiUrl(MovieSearchApi, q, provider, fallback);
            return await GetDataAsync<List<MovieSearchResult>>(apiUrl, true, cancellationToken);
        }

        private async Task<T> GetDataAsync<T>(string url, bool requireAuth,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Add General Headers.
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", _metaTubeConfiguration.DefaultUserAgent);

            // Set API Authorization Token.
            if (requireAuth && !string.IsNullOrWhiteSpace(_metaTubeConfiguration.Token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _metaTubeConfiguration.Token);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Nullable forgiving reason:
            // Response is unlikely to be null.
            // If it happens to be null, an exception is planed to be thrown either way.
            var apiResponse = (await response.Content!
                .ReadFromJsonAsync<ResponseInfo<T>>(cancellationToken: cancellationToken).ConfigureAwait(false))!;

            // EnsureSuccessStatusCode ignoring reason:
            // When the status is unsuccessful, the API response contains error details.
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API request error: {apiResponse.Error.Code} ({apiResponse.Error?.Message})");
            }

            // Note: data field must not be null if there are no errors.
            return apiResponse.Data == null ? throw new Exception("Response data field is null") : apiResponse.Data;
        }
    }
}
