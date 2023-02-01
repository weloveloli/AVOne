// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable

using AVOne;

namespace AVOne.Impl.Providers.Metatube.Models
{
    using System.Text.Json.Serialization;

    public class ActorSearchResult : ProviderInfo
    {
        [JsonPropertyName("images")]
        public string[] Images { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}

