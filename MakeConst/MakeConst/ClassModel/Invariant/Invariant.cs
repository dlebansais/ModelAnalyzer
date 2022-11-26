namespace DemoAnalyzer;

public class Invariant : IInvariant
{
    public required string Text { get; init; }
    public required IField Field { get; init; }
    public required string Operator { get; init; }
    public required int ConstantValue { get; init; }

    public string FieldName { get { return Field.FieldName.Name; } }
}
