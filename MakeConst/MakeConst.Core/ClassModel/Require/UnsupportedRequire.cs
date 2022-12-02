namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedRequire : IRequire
{
    public required string Text { get; init; }
    public required Location Location { get; init; }
}
