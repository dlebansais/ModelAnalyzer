namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("{ParameterName.Name}")]
public class Parameter : IParameter, IVariable
{
    required public ParameterName ParameterName { get; init; }
    public string Name { get { return ParameterName.Name; } }
}
