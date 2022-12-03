namespace DemoAnalyzer;

internal class Invariant : IInvariant
{
    required public string Text { get; init; }
    required public IExpression BooleanExpression { get; init; }

    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}
