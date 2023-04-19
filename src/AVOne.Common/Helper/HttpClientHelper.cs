// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Common.Helper
{
    using AVOne.Configuration;
    using AVOne.Constants;

    public class HttpClientHelper
    {
        public HttpClientHelper(IConfigurationManager manager, IHttpClientFactory httpClientFactory)
        {
            _manager = manager;
            _httpClientFactory = httpClientFactory;
        }
        private (HttpClient httpClient, long version) _httpClientDefault;
        private (HttpClient httpClient, long version) _httpClientDownload;
        private readonly IConfigurationManager _manager;
        private readonly IHttpClientFactory _httpClientFactory;

        public virtual HttpClient GetHttpClient(string name = HttpClientNames.Default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }
            if (name != HttpClientNames.Default && name != HttpClientNames.Download)
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be {name}.", nameof(name));
            }
            if (name == HttpClientNames.Default)
            {
                if (_httpClientDefault == default || _httpClientDefault.version < _manager.CommonConfiguration.Verion)
                {
                    _httpClientDefault = (_httpClientFactory.CreateClient(HttpClientNames.Default), _manager.CommonConfiguration.Verion);
                }
                return _httpClientDefault.httpClient;
            }
            if (name == HttpClientNames.Download)
            {
                if (_httpClientDefault == default || _httpClientDefault.version < _manager.CommonConfiguration.Verion)
                {
                    _httpClientDownload = (_httpClientFactory.CreateClient(HttpClientNames.Download), _manager.CommonConfiguration.Verion);
                }
                return _httpClientDownload.httpClient;
            }

            throw new Exception("Failed to get http client");
        }
    }
}
