namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

internal class UnsupportedExpression : IUnsupportedExpression
{
    public bool IsSimple => true;
    required public Location Location { get; init; }
}
