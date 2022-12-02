namespace DemoAnalyzer;

using System.Diagnostics;
using Microsoft.CodeAnalysis;

[DebuggerDisplay("{ParameterName.Name}")]
public class UnsupportedParameter : IParameter
{
    required public ParameterName ParameterName { get; init; }
    public string Name { get { return ParameterName.Name; } }
    required public Location Location { get; init; }
}
