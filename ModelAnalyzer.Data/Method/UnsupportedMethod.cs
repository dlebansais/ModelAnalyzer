namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents an unsupported method.
/// </summary>
internal class UnsupportedMethod : IUnsupportedMethod
{
    /// <inheritdoc/>
    public IClassMemberName Name => new MethodName() { Text = "*" };

    /// <inheritdoc/>
    public ExpressionType ReturnType => ExpressionType.Other;

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location Location { get; init; }

    /// <inheritdoc/>
    public IList<IParameter> GetParameters() => new List<IParameter>();

    /// <inheritdoc/>
    public IList<IRequire> GetRequires() => new List<IRequire>();

    /// <inheritdoc/>
    public IList<ILocal> GetLocals() => new List<ILocal>();

    /// <inheritdoc/>
    public ILocal? ResultLocal => null;

    /// <inheritdoc/>
    public IList<IEnsure> GetEnsures() => new List<IEnsure>();
}
