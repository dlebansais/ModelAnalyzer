namespace DemoAnalyzer;

public class VariableValueExpression : IExpression
{
    public required IVariable Variable { get; init; }
}
