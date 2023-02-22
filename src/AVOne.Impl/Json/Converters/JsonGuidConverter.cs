// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Json.Converters
{
    using System;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Converts a GUID object or value to/from JSON.
    /// </summary>
    public class JsonGuidConverter : JsonConverter<Guid>
    {
        /// <inheritdoc />
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.Null
                ? Guid.Empty
                : ReadInternal(ref reader);

        // TODO: optimize by parsing the UTF8 bytes instead of converting to string first
        internal static Guid ReadInternal(ref Utf8JsonReader reader)
            => Guid.Parse(reader.GetString()!); // null got handled higher up the call stack

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
            => WriteInternal(writer, value);

        internal static void WriteInternal(Utf8JsonWriter writer, Guid value)
            => writer.WriteStringValue(value.ToString("N", CultureInfo.InvariantCulture));
    }
}
