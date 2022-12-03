namespace DemoAnalyzer;

using System.Diagnostics;

[DebuggerDisplay("{ParameterName.Name}")]
internal class Parameter : IParameter, IVariable
{
    required public IParameterName ParameterName { get; init; }
    public string Name { get { return ParameterName.Name; } }
}
