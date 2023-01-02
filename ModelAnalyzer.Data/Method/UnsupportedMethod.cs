namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents an unsupported method.
/// </summary>
internal class UnsupportedMethod : IUnsupportedMethod
{
    /// <inheritdoc/>
    public IClassMemberName Name => new MethodName() { Text = "*" };

    /// <inheritdoc/>
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public IList<IRequire> GetRequires() => new List<IRequire>();

    /// <inheritdoc/>
    public IList<ILocal> GetLocals() => new List<ILocal>();

    /// <inheritdoc/>
    public IList<IEnsure> GetEnsures() => new List<IEnsure>();
}
