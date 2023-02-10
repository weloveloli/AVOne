// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
namespace AVOne.Impl.Providers.Metatube.Models
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

