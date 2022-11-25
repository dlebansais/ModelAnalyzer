namespace DemoAnalyzer;

public class Invariant : IInvariant
{
    public required string Text { get; init; }
    public string FieldName { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Constant { get; set; } = string.Empty;
}
