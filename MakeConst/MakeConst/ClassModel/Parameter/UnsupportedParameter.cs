namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;
using System.Diagnostics;

[DebuggerDisplay("{ParameterName.Name}")]
public class UnsupportedParameter : IParameter
{
    public required ParameterName ParameterName { get; init; }
    public string Name { get { return ParameterName.Name; } }
    public required Location Location { get; init; }
}
