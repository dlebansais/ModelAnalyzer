namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

internal class UnsupportedRequire : IUnsupportedRequire
{
    required public string Text { get; init; }
    required public Location Location { get; init; }
}
