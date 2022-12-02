namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedField : IField
{
    required public FieldName FieldName { get; init; }
    public string Name { get { return FieldName.Name; } }
    required public Location Location { get; init; }
}
