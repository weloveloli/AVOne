// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.MetaTube.Configuration
{
    using AVOne.Configuration;

    public class MetaTubeConfigStore : ConfigurationStore
    {
        /// <summary>
        /// The name of the configuration in the storage.
        /// </summary>
        public const string StoreKey = "metatube";

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaTubeConfigStore"/> class.
        /// </summary>
        public MetaTubeConfigStore()
        {
            ConfigurationType = typeof(MetaTubeConfiguration);
            Key = StoreKey;
        }
    }
}
