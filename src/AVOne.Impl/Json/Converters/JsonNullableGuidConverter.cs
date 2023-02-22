// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Json.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Converts a GUID object or value to/from JSON.
    /// </summary>
    public class JsonNullableGuidConverter : JsonConverter<Guid?>
    {
        /// <inheritdoc />
        public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonGuidConverter.ReadInternal(ref reader);

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
        {
            // null got handled higher up the call stack
            var val = value!.Value;
            if (val.Equals(default))
            {
                writer.WriteNullValue();
            }
            else
            {
                JsonGuidConverter.WriteInternal(writer, val);
            }
        }
    }
}
