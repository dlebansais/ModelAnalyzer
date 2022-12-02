namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedMethod : IMethod
{
    public required MethodName MethodName { get; init; }
    public required Location Location { get; init; }
}
