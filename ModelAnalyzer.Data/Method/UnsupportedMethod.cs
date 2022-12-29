namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported method.
/// </summary>
public class UnsupportedMethod : IUnsupportedMethod
{
    /// <inheritdoc/>
    public MethodName Name => new MethodName() { Text = "*" };

    /// <inheritdoc/>
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<IRequire> GetRequires() => new List<IRequire>().AsReadOnly();

    /// <inheritdoc/>
    public IReadOnlyList<IEnsure> GetEnsures() => new List<IEnsure>().AsReadOnly();
}
