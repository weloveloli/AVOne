// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

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

    public interface IApplicationConfigs
    {
        public MovieID MovieID { get; set; }

        public FileConfig File { get; set; }

    }
}
