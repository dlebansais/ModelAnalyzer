namespace DemoAnalyzer;

public class BinaryExpression : IExpression
{
    public required IExpression Left { get; init; }
    public required string OperatorText { get; init; }
    public required IExpression Right { get; init; }
}
