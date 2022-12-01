namespace DemoAnalyzer;

public class LiteralValueExpression : IExpression
{
    public bool IsSimple => true;
    public required int Value { get; init; }

    public override string ToString()
    {
        return $"{Value}";
    }
}
