namespace DemoAnalyzer;

public class ParenthesizedExpression : IExpression
{
    public bool IsSimple => false;
    public required IExpression Inside { get; init; }

    public override string ToString()
    {
        return Inside.ToString();
    }
}
