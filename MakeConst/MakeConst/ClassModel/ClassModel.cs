namespace DemoAnalyzer;

using System.Collections.Generic;

public record ClassModel
{
    public required string Name { get; init; }
    public Dictionary<FieldName, Field> FieldTable { get; } = new();
    public Dictionary<MethodName, Method> MethodTable { get; } = new();
}
