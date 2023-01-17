// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Naming
{
    using AVOne.Impl.Constants;
    using AVOne.Providers;
    using Emby.Naming.Common;

    public class JellyfinNamingOptionProvider : INamingOptionProvider
    {

        private readonly INamingOptions _namingOptions;
        public JellyfinNamingOptionProvider()
        {
            this._namingOptions = new JellyfinNamingOptions();
        }

        public string Name => OfficialProviderNames.ProviderJellifin;

        public INamingOptions GetNamingOption() => this._namingOptions;
    }

    internal class JellyfinNamingOptions : INamingOptions
    {
        private readonly NamingOptions _options;

        public JellyfinNamingOptions()
        {
            this._options = new NamingOptions();
        }

        /// <summary>
        /// Gets or sets list of video file extensions.
        /// </summary>
        public string[] VideoFileExtensions => _options.VideoFileExtensions;
    }
}
