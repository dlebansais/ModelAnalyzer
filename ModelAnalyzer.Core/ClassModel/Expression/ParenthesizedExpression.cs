namespace DemoAnalyzer;

internal class ParenthesizedExpression : IExpression
{
    public bool IsSimple => false;
    required public IExpression Inside { get; init; }

    public override string ToString()
    {
        return Inside.ToString();
    }
}
