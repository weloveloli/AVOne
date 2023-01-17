// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.

namespace AVOne.Impl.Providers.Naming
{
    using AVOne.Providers;

    public class JellyfinNamingOptionProvider : INamingOptionProvider
    {
        public JellyfinNamingOptionProvider()
        {

        }

        public string Name => throw new NotImplementedException();

        public INamingOption GetNamingOption()
        {
            return new NamingOption();
        }
    }
}
