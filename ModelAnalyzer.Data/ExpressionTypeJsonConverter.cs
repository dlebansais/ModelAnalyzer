namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

/// <summary>
/// Represents a converter for the <see cref="ExpressionType"/> type.
/// </summary>
public class ExpressionTypeJsonConverter : JsonConverter<ExpressionType>
{
    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, ExpressionType? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("Name");
        writer.WriteValue(value?.Name);
        writer.WriteEndObject();
    }

    /// <inheritdoc/>
    public override ExpressionType? ReadJson(JsonReader reader, Type objectType, ExpressionType? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        Debug.Assert(reader.TokenType == JsonToken.StartObject);

        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.PropertyName);
        Debug.Assert(reader.ValueType == typeof(string));
        Debug.Assert(reader.Path == "Name");

        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.String);
        Debug.Assert(reader.ValueType == typeof(string));

        string? Name = reader.Value as string;

        reader.Read();
        Debug.Assert(reader.TokenType == JsonToken.EndObject);

        Dictionary<string?, ExpressionType> SwitchTable = new()
        {
            { string.Empty, ExpressionType.Other },
            { "void", ExpressionType.Void },
            { "bool", ExpressionType.Boolean },
            { "int", ExpressionType.Integer },
            { "double", ExpressionType.FloatingPoint },
        };

        if (SwitchTable.ContainsKey(Name))
            return SwitchTable[Name];
        else
            return new ExpressionType(null!);
    }
}
