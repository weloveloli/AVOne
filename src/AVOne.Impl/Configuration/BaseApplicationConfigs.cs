// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Configuration
{
    using AVOne.Configuration;

    public class BaseApplicationConfigs : IApplicationConfigs, IOfficialProvidersConfiguration
    {
        public MetadataOptions[] MetadataOptions { get; set; }
        public FileConfig File { get; set; }
        public MovieID MovieID { get; set; }
        public MetaTubeConfiguration MetaTube { get; set; }

        public BaseApplicationConfigs()
        {
            MetadataOptions = Array.Empty<MetadataOptions>();
            File = new FileConfig();
            MovieID = new MovieID();
            MetaTube = new MetaTubeConfiguration();
        }
    }
}
