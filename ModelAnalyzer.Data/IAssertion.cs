namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about an assertion.
/// </summary>
public interface IAssertion
{
    /// <summary>
    /// Gets the assertion text.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Gets the assertion location.
    /// </summary>
    Location Location { get; }

    /// <summary>
    /// Gets the assertion expression.
    /// </summary>
    IExpression BooleanExpression { get; }
}
