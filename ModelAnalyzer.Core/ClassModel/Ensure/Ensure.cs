namespace DemoAnalyzer;

/// <summary>
/// Represents a guarantee when a method returns.
/// </summary>
public class Ensure : IEnsure
{
    /// <summary>
    /// Gets the text of the assertion.
    /// </summary>
    required public string Text { get; init; }

    /// <summary>
    /// Gets the expression guaranteed to be true.
    /// </summary>
    required public IExpression BooleanExpression { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}
