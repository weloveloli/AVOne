// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable
using AVOne;

namespace AVOne.Impl.Providers.Metatube.Models
{
    using System.Text.Json.Serialization;

    public class ErrorInfo
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
