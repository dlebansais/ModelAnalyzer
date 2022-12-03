namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

internal class UnsupportedStatement : IUnsupportedStatement
{
    required public Location Location { get; init; }
}
