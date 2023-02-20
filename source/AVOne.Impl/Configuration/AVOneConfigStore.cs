// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Configuration
{
    using MediaBrowser.Common.Configuration;

    public class AVOneConfigStore : ConfigurationStore
    {
        /// <summary>
        /// The name of the configuration in the storage.
        /// </summary>
        public const string StoreKey = "avone";

        /// <summary>
        /// Initializes a new instance of the <see cref="AVOneConfigStore"/> class.
        /// </summary>
        public AVOneConfigStore()
        {
            ConfigurationType = typeof(AVOneConfiguration);
            Key = StoreKey;
        }
    }
}
