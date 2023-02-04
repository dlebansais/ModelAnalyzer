namespace ModelAnalyzer;

internal record ArraySize
{
    public static ArraySize Invalid { get; } = new ArraySize() { Size = int.MinValue };
    public static ArraySize Unknown { get; } = new ArraySize() { Size = -1 };

    public int Size { get; set; }

    public bool IsValid { get => Size >= -1; }
    public bool IsKnown { get => Size >= 0; }
}
