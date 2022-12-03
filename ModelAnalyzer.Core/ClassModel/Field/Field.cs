namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("{FieldName.Name}")]
internal class Field : IField
{
    required public IFieldName FieldName { get; init; }
    public string Name { get { return FieldName.Name; } }
}
