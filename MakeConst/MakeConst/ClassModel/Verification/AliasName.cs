namespace DemoAnalyzer;

public record AliasName
{
    public required string BaseName { get; init; }
    public string Alias { get { return $"{BaseName}_{Index}"; } }

    public void Increment()
    {
        Index++;
    }

    private int Index;
}
