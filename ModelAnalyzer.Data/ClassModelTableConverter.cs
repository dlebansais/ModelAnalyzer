namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

/// <summary>
/// Represents a class model table converter.
/// </summary>
internal class ClassModelTableConverter : JsonConverter<ClassModelTable>
{
    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, ClassModelTable? value, JsonSerializer serializer)
    {
        Debug.Assert(value is not null);

        JsonSerializerSettings Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
        Settings.Converters.Add(new ClassNameConverter());

        List<KeyValuePair<ClassName, ClassModel>> EntryList = new List<KeyValuePair<ClassName, ClassModel>>(value);
        string JsonText = JsonConvert.SerializeObject(EntryList, Settings);

        writer.WriteValue(JsonText);
    }

    /// <inheritdoc/>
    public override ClassModelTable? ReadJson(JsonReader reader, Type objectType, ClassModelTable? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        Debug.Assert(reader.Value is string);

        string StringValue = (string)reader.Value!;
        JsonSerializerSettings Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
        Settings.Converters.Add(new ClassNameConverter());

        List<KeyValuePair<ClassName, ClassModel>>? EntryList = JsonConvert.DeserializeObject<List<KeyValuePair<ClassName, ClassModel>>>(StringValue, Settings);

        Debug.Assert(EntryList is not null);

        ClassModelTable Result = new();

        foreach (KeyValuePair<ClassName, ClassModel> Entry in EntryList!)
            Result.Add(Entry.Key, Entry.Value);

        return Result;
    }
}
