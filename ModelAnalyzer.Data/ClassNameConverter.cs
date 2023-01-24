namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
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
        if (reader.Value is string StringValue)
        {
            string[] Splitted = StringValue.Split('.');

            if (Splitted.Length == 0)
                return ClassName.Empty;
            else if (Splitted.Length == 1)
                return ClassName.FromSimpleString(Splitted[0]);
            else
            {
                List<string> Namespace = new List<string>(Splitted);
                Namespace.RemoveAt(Namespace.Count - 1);

                return new ClassName() { Namespace = Namespace, Text = Splitted.Last() };
            }
        }
        else
            return null;
    }
}
