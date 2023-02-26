namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents the name of a variable.
/// </summary>
[DebuggerDisplay("{Text}")]
internal class CodeVariableName : IVariableName
{
    /// <inheritdoc/>
    required public string Text { get; init; }
}
