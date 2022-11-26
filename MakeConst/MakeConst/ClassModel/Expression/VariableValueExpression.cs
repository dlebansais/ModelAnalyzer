namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("{Variable.Name}")]
public class VariableValueExpression : IExpression
{
    public required IVariable Variable { get; init; }
}
