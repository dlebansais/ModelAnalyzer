namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedInvariant : IInvariant
{
    public required string Text { get; init; }
    public required Location Location { get; init; }
}
