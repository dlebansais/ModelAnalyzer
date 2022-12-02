namespace DemoAnalyzer;

public class Require : IRequire
{
    public required string Text { get; init; }
    public required IExpression BooleanExpression { get; init; }

    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}
