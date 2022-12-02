namespace DemoAnalyzer;

public record FieldName(string Name) : IClassMemberName
{
    public static FieldName UnsupportedFieldName { get; } = new("*");
}
