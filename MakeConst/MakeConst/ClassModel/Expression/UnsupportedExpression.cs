namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedExpression : IExpression
{
    public required Location Location { get; init; }
}
