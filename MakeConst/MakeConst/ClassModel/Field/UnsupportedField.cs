namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedField : IField
{
    public required FieldName Name { get; init; }
    public required Location Location { get; init; }
}
