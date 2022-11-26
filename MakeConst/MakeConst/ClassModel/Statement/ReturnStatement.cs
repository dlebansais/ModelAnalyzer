namespace DemoAnalyzer;

public class ReturnStatement : IStatement
{
    public required IExpression Expression { get; init; }
}
