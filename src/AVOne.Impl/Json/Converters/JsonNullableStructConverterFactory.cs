// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Impl.Json.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Json nullable struct converter factory.
    /// </summary>
    public class JsonNullableStructConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType
                   && typeToConvert.GetGenericTypeDefinition() == typeof(Nullable<>)
                   && typeToConvert.GenericTypeArguments[0].IsValueType;
        }

        /// <inheritdoc />
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var structType = typeToConvert.GenericTypeArguments[0];
            return (JsonConverter?)Activator.CreateInstance(typeof(JsonNullableStructConverter<>).MakeGenericType(structType));
        }
    }
}
