// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable

namespace AVOne.Configuration
{

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
        }
        public MovieID MovieID { get; set; }

        public FileConfig File { get; set; }

        public MetadataOptions[] MetadataOptions { get; set; }
    }
}
