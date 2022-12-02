namespace DemoAnalyzer;

public class Ensure : IEnsure
{
    public required string Text { get; init; }
    public required IExpression BooleanExpression { get; init; }

    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}
