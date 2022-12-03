namespace DemoAnalyzer;

/// <summary>
/// Represents a requirement assertion.
/// </summary>
internal class Require : IRequire
{
    /// <inheritdoc/>
    required public string Text { get; init; }

    /// <summary>
    /// Gets the expression representing the requirement.
    /// </summary>
    required public IExpression BooleanExpression { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return BooleanExpression.ToString();
    }
}
