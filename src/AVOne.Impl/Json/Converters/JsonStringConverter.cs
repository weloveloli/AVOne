// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Json.Converters
{
    using System;
    using System.Buffers;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Converter to allow the serializer to read strings.
    /// </summary>
    public class JsonStringConverter : JsonConverter<string?>
    {
        /// <inheritdoc />
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.String ? reader.GetString() : GetRawValue(reader);

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);

        private static string GetRawValue(Utf8JsonReader reader)
        {
            var utf8Bytes = reader.HasValueSequence
                ? reader.ValueSequence.ToArray()
                : reader.ValueSpan;
            return Encoding.UTF8.GetString(utf8Bytes);
        }
    }
}
