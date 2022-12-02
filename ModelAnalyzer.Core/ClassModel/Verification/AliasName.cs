namespace DemoAnalyzer;

public record AliasName
{
    public AliasName()
    {
        Index = 0;
    }

    public required string BaseName { get; init; }
    public string Alias { get { return $"{BaseName}_{Index}"; } }

    public void Increment()
    {
        Index++;
    }

    public void Merge(AliasName other, out bool isUpdated)
    {
        if (other.Index != Index)
        {
            isUpdated = true;
            Increment();
        }
        else
            isUpdated = false;
    }

    private int Index;
}
