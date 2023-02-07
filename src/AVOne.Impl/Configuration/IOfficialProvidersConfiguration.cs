// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable
namespace AVOne.Configuration
{
    public interface IOfficialProvidersConfiguration
    {
        public MetaTubeConfiguration MetaTube { get; }
    }

    public class MetaTubeConfiguration
    {
        public string Server { get; set; } = "localhost";
        public int DefaultImageQuality { get; set; } = -1;
        public double PrimaryImageRatio { get; set; } = 90;
        public string DefaultUserAgent => $"AVOne";
        public string Token { get; set; } = string.Empty;
    }
}
