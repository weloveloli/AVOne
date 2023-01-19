// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable
namespace AVOne.Configuration
{
    public class IOfficialProvidersConfiguration
    {
        public MetaTubeConfiguration MetaTube { get; set; }
    }

    public class MetaTubeConfiguration
    {
        public string Server { get; set; }
        public string DefaultImageQuality { get; set; }
        public double PrimaryImageRatio { get; set; }
        public string DefaultUserAgent { get; set; }
        public string Token { get; set; }
    }
}
