namespace DemoAnalyzer;

public class LiteralValueExpression : IExpression
{
    public bool IsSimple => true;
    required public int Value { get; init; }

    public override string ToString()
    {
        return $"{Value}";
    }
}
