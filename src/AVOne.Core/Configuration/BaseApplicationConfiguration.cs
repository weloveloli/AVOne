﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

#nullable disable

namespace AVOne.Configuration
{
    using AVOne.Models.Updates;

    public class MovieID
    {
        public string ignore_whole_word { get; set; } = "144P;240P;360P;480P;720P;1080P;2K;4K";
        public string ignore_regex { get; set; } = "\\w+2048\\.com;Carib(beancom)?;[^a-z\\d](f?hd|lt)[^a-z\\d]";
    }
    public class FileConfig
    {
        public string media_ext { get; set; } = "3gp;avi;f4v;flv;iso;m2ts;m4v;mkv;mov;mp4;mpeg;rm;rmvb;ts;vob;webm;wmv";
        public string ignore_folder { get; set; } = "cleaned";
    }

    public class ProviderConfig
    {
        public string NameOptionProvider { get; set; } = "Jellyfin";
        public string NameResolveProvider { get; set; } = "Jellyfin";
        public string[] ScanMetaDataProvidersFilter { get; set; } = new string[] { };
        public string[] ImageMetaDataProvidersFilter { get; set; } = new string[] { };
        public string Proxy { get; set; }
    }

    public class DownloadConfig
    {
        public string DefaultDownloadDir { get; set; } = OperatingSystem.IsWindows() ? "d://downloads" : "/download";
        public int DefaultDownloadThreadCount { get; set; } = 8;
        public int DefaultRetryCount { get; set; } = 4;
        public int DefaultRetryWait { get; set; } = 500;
        public string Proxy { get; set; }
    }

    public class FFmpegConfig
    {
        public string FFmpegPath { get; set; }
    }

    /// <summary>
    /// Class MetadataOptions.
    /// </summary>
    public class MetadataOptions
    {
        public MetadataOptions()
        {
            DisabledMetadataSavers = Array.Empty<string>();
            LocalMetadataReaderOrder = Array.Empty<string>();
            DisabledMetadataFetchers = Array.Empty<string>();
            MetadataFetcherOrder = Array.Empty<string>();
            DisabledImageFetchers = Array.Empty<string>();
            ImageFetcherOrder = Array.Empty<string>();
        }

        public string ItemType { get; set; }

        public string[] DisabledMetadataSavers { get; set; }

        public string[] LocalMetadataReaderOrder { get; set; }

        public string[] DisabledMetadataFetchers { get; set; }

        public string[] MetadataFetcherOrder { get; set; }

        public string[] DisabledImageFetchers { get; set; }

        public string[] ImageFetcherOrder { get; set; }
    }

    public class BaseApplicationConfiguration
    {
        public BaseApplicationConfiguration()
        {
            MovieID = new MovieID();
            File = new FileConfig();
            MetadataOptions = Array.Empty<MetadataOptions>();
            ProviderConfig = new ProviderConfig();
            DownloadConfig = new DownloadConfig();
            FFmpegConfig = new FFmpegConfig();
        }
        public MovieID MovieID { get; set; }

        public FileConfig File { get; set; }

        public MetadataOptions[] MetadataOptions { get; set; }

        public ProviderConfig ProviderConfig { get; set; }
        public DownloadConfig DownloadConfig { get; set; }
        public FFmpegConfig FFmpegConfig { get; set; }
        public NameValue[] ContentTypes { get; set; } = Array.Empty<NameValue>();

        /// <summary>
        /// Gets or sets a value indicating whether [enable case sensitive item ids].
        /// </summary>
        /// <value><c>true</c> if [enable case sensitive item ids]; otherwise, <c>false</c>.</value>
        public bool EnableCaseSensitiveItemIds { get; set; } = true;

        public List<RepositoryInfo> PluginRepositories { get; set; } = new List<RepositoryInfo>();

        public long Verion { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
    }
}
