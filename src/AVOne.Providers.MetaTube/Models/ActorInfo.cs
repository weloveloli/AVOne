﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
namespace AVOne.Providers.Metatube.Models
{
#nullable disable

    using System.Text.Json.Serialization;

    public class ActorInfo : ActorSearchResult
    {
        [JsonPropertyName("aliases")]
        public string[] Aliases { get; set; }

        [JsonPropertyName("birthday")]
        public DateTime Birthday { get; set; }

        [JsonPropertyName("blood_type")]
        public string BloodType { get; set; }

        [JsonPropertyName("cup_size")]
        public string CupSize { get; set; }

        [JsonPropertyName("debut_date")]
        public DateTime DebutDate { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("hobby")]
        public string Hobby { get; set; }

        [JsonPropertyName("skill")]
        public string Skill { get; set; }

        [JsonPropertyName("measurements")]
        public string Measurements { get; set; }

        [JsonPropertyName("nationality")]
        public string Nationality { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }
    }
}
