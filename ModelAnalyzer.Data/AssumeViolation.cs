namespace ModelAnalyzer;

/// <summary>
/// Represents a flow check violation.
/// </summary>
internal class AssumeViolation : IAssumeViolation
{
    /// <inheritdoc/>
    required public IMethod? Method { get; init; }

    /// <inheritdoc/>
    required public string Text { get; init; }
}
