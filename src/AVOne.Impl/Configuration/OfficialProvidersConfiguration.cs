// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable
namespace AVOne.Configuration
{
    using AVOne.Constants;

    public class IOfficialProvidersConfiguration
    {
        public MetaTubeConfiguration MetaTube { get; set; }
    }

    public class MetaTubeConfiguration
    {
        public string Server { get; set; } = "localhost";
        public int DefaultImageQuality { get; set; } = -1;
        public double PrimaryImageRatio { get; set; } = 90;
        public string DefaultUserAgent => $"AVOne/{AVOneConstants.AVOneVersion}";
        public string Token { get; set; } = string.Empty;
    }
}
