namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedEnsure : IEnsure
{
    public required string Text { get; init; }
    public required Location Location { get; init; }
}
