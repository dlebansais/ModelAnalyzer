namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

internal class UnsupportedMethod : IUnsupportedMethod
{
    required public IMethodName MethodName { get; init; }
    required public Location Location { get; init; }
}
