using Microsoft.CodeAnalysis;

namespace DemoAnalyzer;

public class UnsupportedStatement : IStatement
{
    public required Location Location { get; init; }
}
