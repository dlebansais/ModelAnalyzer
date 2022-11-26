namespace DemoAnalyzer;

public class Invariant : IInvariant
{
    public required string Text { get; init; }
    public required Field Field { get; init; }
    public required string Operator { get; init; }
    public required int ConstantValue { get; init; }

    public string FieldName { get { return Field.Name.Name; } }
}
