namespace DemoAnalyzer;

public class AssignmentStatement : IStatement
{
    public required IField Destination { get; init; }
    public required IExpression Expression { get; init; }
}
