namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedStatement : IStatement
{
    required public Location Location { get; init; }
}
