namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Represents a class name converter.
/// </summary>
internal class ClassNameConverter : JsonConverter<ClassName>
{
    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, ClassName? value, JsonSerializer serializer)
    {
        string? StringValue = value?.ToString();
        writer.WriteValue(StringValue);
    }

    /// <inheritdoc/>
    public override ClassName? ReadJson(JsonReader reader, Type objectType, ClassName? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        Debug.Assert(reader.Value is string);

        string StringValue = (string)reader.Value!;

        if (StringValue == string.Empty)
            return ClassName.Empty;
        else
        {
            string[] Splitted = StringValue.Split('.');
            Debug.Assert(Splitted.Length >= 1);

            if (Splitted.Length == 1)
                return ClassName.FromSimpleString(Splitted[0]);
            else
            {
                List<string> Namespace = new List<string>(Splitted);
                Namespace.RemoveAt(Namespace.Count - 1);

                return new ClassName() { Namespace = Namespace, Text = Splitted.Last() };
            }
        }
    }
}
