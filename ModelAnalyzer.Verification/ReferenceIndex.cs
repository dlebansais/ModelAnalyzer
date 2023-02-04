namespace ModelAnalyzer;

internal record ReferenceIndex
{
    public static ReferenceIndex Null { get; } = new ReferenceIndex() { Internal = 0 };
    public static ReferenceIndex Root { get; } = new ReferenceIndex() { Internal = 1 };
    public static ReferenceIndex First { get; } = new ReferenceIndex() { Internal = 2 };

    public int Internal { get; set; }

    public ReferenceIndex Increment()
    {
        ReferenceIndex Result = new ReferenceIndex() { Internal = Internal };

        Internal++;

        return Result;
    }
}
