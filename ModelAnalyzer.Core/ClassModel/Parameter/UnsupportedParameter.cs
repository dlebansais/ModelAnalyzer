namespace DemoAnalyzer;

using System.Diagnostics;
using Microsoft.CodeAnalysis;

[DebuggerDisplay("{ParameterName.Name}")]
internal class UnsupportedParameter : IUnsupportedParameter
{
    required public IParameterName ParameterName { get; init; }
    public string Name { get { return ParameterName.Name; } }
    required public Location Location { get; init; }
}
