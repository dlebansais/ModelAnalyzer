namespace DemoAnalyzer;

public class VariableValueExpression : IExpression
{
    public bool IsSimple => true;
    required public IVariable Variable { get; init; }

    public override string ToString()
    {
        return $"{Variable.Name}";
    }
}
