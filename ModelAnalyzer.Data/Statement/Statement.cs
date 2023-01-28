namespace ModelAnalyzer;

/// <summary>
/// Represents a statement.
/// </summary>
internal abstract class Statement : IStatement
{
    /// <summary>
    /// Gets or sets the location Id.
    /// </summary>
    public abstract LocationId LocationId { get; set; }
}
