// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Metatube.Models
{
#nullable disable

    using System.Text.Json.Serialization;

    public class ActorSearchResult : ProviderInfo
    {
        [JsonPropertyName("images")]
        public string[] Images { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}

