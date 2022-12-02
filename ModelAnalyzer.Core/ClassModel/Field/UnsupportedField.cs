namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedField : IField
{
    public required FieldName FieldName { get; init; }
    public string Name { get { return FieldName.Name; } }
    public required Location Location { get; init; }
}
