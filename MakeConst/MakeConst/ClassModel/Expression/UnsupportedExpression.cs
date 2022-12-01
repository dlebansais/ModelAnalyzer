namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedExpression : IExpression
{
    public bool IsSimple => true;
    public required Location Location { get; init; }
}
