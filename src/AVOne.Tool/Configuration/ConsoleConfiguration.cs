// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Tool.Configuration
{
    using AVOne.Configuration;
    using AVOne.Providers.MetaTube.Configuration;

    public class ConsoleConfiguration : BaseApplicationConfiguration, IMetaTubeConfiguration
    {
        public ConsoleConfiguration()
        {
            MetaTube = new MetaTubeConfiguration();
        }

        public MetaTubeConfiguration MetaTube { get; set; }
    }
}
