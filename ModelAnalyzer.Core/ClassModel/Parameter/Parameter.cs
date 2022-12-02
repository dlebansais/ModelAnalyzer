namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("{ParameterName.Name}")]
public class Parameter : IParameter, IVariable
{
    public required ParameterName ParameterName { get; init; }
    public string Name { get { return ParameterName.Name; } }
}
