// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
namespace AVOne.Impl.Providers.Metatube.Models
{
#nullable disable
    using System.Text.Json.Serialization;

    public class ProviderInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("provider")]
        public string Provider { get; set; }

        [JsonPropertyName("homepage")]
        public string Homepage { get; set; }
    }
}
