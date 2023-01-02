namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a method name.
/// </summary>
[DebuggerDisplay("{Text}")]
internal record MethodName : IClassMemberName
{
    /// <summary>
    /// Gets the method name.
    /// </summary>
    required public string Text { get; init; }
}
