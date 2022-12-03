namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

internal class UnsupportedField : IUnsupportedField
{
    required public IFieldName FieldName { get; init; }
    public string Name { get { return FieldName.Name; } }
    required public Location Location { get; init; }
}
