namespace DemoAnalyzer;

/// <summary>
/// Represents text patterns for a class model.
/// </summary>
public static class Modeling
{
    /// <summary>
    /// Gets the pattern to turn off modeling.
    /// </summary>
    public const string None = "No model";

    /// <summary>
    /// Gets the pattern to declare an invariant.
    /// </summary>
    public const string Invariant = "Invariant: ";

    /// <summary>
    /// Gets the pattern to declare a requirement assertion.
    /// </summary>
    public const string Require = "Require: ";

    /// <summary>
    /// Gets the pattern to declare a guarantee ssertion.
    /// </summary>
    public const string Ensure = "Ensure: ";
}
