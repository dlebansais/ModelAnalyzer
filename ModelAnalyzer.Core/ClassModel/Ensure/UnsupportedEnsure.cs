namespace DemoAnalyzer;

using Microsoft.CodeAnalysis;

public class UnsupportedEnsure : IEnsure
{
    required public string Text { get; init; }
    required public Location Location { get; init; }
}
