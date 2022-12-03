namespace DemoAnalyzer;

internal class Require : IRequire
{
    required public string Text { get; init; }
    required public IExpression BooleanExpression { get; init; }

    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}
