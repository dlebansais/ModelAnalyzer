namespace ModelAnalyzer;

/// <summary>
/// Provides information about items with a name.
/// </summary>
/// <typeparam name="TName">The item name type.</typeparam>
public interface INameable<TName>
{
    /// <summary>
    /// Gets the item name.
    /// </summary>
    TName Name { get; }
}
