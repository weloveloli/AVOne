// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Test
{
    using AVOne.Configuration;

    public class TestApplicationConfigs : IApplicationConfigs
    {
        public TestApplicationConfigs()
        {
            MovieID = new MovieID();
            File = new FileConfig();
        }
        public MovieID MovieID { get ; set ; }
        public FileConfig File { get; set; }
    }
}
