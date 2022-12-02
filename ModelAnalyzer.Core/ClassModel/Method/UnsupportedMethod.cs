namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedMethod : IMethod
{
    required public MethodName MethodName { get; init; }
    required public Location Location { get; init; }
}
