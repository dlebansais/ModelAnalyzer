namespace ModelAnalyzer;

/// <summary>
/// Represents a location identifier.
/// </summary>
internal record LocationId
{
    /// <summary>
    /// Gets the Id for no location.
    /// </summary>
    public static LocationId None { get; } = new() { Id = 0 };

    /// <summary>
    /// Gets or sets the Id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Creates a new location Id.
    /// </summary>
    public static LocationId CreateNew()
    {
        return new LocationId() { Id = ++GlobalId };
    }

    /// <summary>
    /// Resets the ID seed.
    /// </summary>
    public static void Reset()
    {
        // TODO: replace with a correct system in which expressions and statements keep their location ID properly.
        GlobalId = 0;
    }

    private static long GlobalId = 0;
}
