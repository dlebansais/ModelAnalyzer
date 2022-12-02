namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("{FieldName.Name}")]
public class Field : IField
{
    public required FieldName FieldName { get; init; }
    public string Name { get { return FieldName.Name; } }
}
