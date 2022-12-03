namespace DemoAnalyzer;

internal record FieldName(string Name) : IFieldName
{
    public static FieldName UnsupportedFieldName { get; } = new("*");
}
