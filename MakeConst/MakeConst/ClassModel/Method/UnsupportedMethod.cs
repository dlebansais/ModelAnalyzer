namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedMethod : IMethod
{
    public required MethodName Name { get; init; }
    public required Location Location { get; init; }
}
