namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

internal class UnsupportedInvariant : IUnsupportedInvariant
{
    required public string Text { get; init; }
    required public Location Location { get; init; }
}
