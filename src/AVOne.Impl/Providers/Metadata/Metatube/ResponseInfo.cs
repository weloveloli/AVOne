// Copyright (c) 2023 Weloveloli. All rights reserved.
// Licensed under the Apache V2.0 License.
#nullable disable
namespace AVOne.Impl.Providers.Metadata.Metatube
{
    using System.Text.Json.Serialization;

    public class ResponseInfo<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("error")]
        public ErrorInfo Error { get; set; }
    }
}

