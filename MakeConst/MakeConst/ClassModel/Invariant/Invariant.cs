namespace DemoAnalyzer;

public class Invariant : IInvariant
{
    public required string Text { get; init; }
    public required string FieldName { get; init; }
    public required string Operator { get; init; }
    public required int ConstantValue { get; init; }
}
