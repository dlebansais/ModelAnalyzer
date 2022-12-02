namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedRequire : IRequire
{
    required public string Text { get; init; }
    required public Location Location { get; init; }
}
