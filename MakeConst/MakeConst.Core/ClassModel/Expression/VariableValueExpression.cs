namespace DemoAnalyzer;

public class VariableValueExpression : IExpression
{
    public bool IsSimple => true;
    public required IVariable Variable { get; init; }

    public override string ToString()
    {
        return $"{Variable.Name}";
    }
}
