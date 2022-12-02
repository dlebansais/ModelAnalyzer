namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedInvariant : IInvariant
{
    required public string Text { get; init; }
    required public Location Location { get; init; }
}
