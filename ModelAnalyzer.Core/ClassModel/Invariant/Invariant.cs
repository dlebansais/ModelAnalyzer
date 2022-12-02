namespace DemoAnalyzer;

public class Invariant : IInvariant
{
    public required string Text { get; init; }
    public required IExpression BooleanExpression { get; init; }

    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}
