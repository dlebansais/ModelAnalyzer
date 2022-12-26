namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported method.
/// </summary>
public class UnsupportedMethod : IUnsupportedMethod
{
    /// <inheritdoc/>
    public MethodName MethodName => new MethodName() { Text = "*" };

    /// <inheritdoc/>
    required public Location Location { get; init; }
}
