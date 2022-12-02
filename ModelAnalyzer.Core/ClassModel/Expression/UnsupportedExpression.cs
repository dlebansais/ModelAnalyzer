namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedExpression : IExpression
{
    public bool IsSimple => true;
    required public Location Location { get; init; }
}
