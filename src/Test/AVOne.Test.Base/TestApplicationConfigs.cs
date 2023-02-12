// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Test.Base
{
    using AVOne.Configuration;
    using AVOne.Impl.Configuration;

    public class TestApplicationConfigs : BaseApplicationConfiguration, IOfficialProvidersConfiguration
    {
        public TestApplicationConfigs()
        {
            MetaTube = new MetaTubeConfiguration();
        }
        public MetaTubeConfiguration MetaTube { get; set; }
    }
}
