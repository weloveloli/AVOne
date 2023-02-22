// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Json.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Enum flag to json array converter.
    /// </summary>
    /// <typeparam name="T">The type of enum.</typeparam>
    public class JsonFlagEnumConverter<T> : JsonConverter<T>
        where T : struct, Enum
    {
        private static readonly T[] _enumValues = (T[])Enum.GetValues(typeof(T));

        /// <inheritdoc />
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var enumValue in _enumValues)
            {
                if (value.HasFlag(enumValue))
                {
                    writer.WriteStringValue(enumValue.ToString());
                }
            }

            writer.WriteEndArray();
        }
    }
}

