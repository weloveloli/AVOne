// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Metatube.Models
{
#nullable disable
    using System.Text.Json.Serialization;

    public class ResponseInfo<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("error")]
        public ErrorInfo Error { get; set; }
    }
}

