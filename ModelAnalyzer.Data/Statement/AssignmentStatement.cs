namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents an assignment statement.
/// </summary>
[DebuggerDisplay("{DestinationName.Text} = {Expression}")]
internal class AssignmentStatement : Statement
{
    /// <inheritdoc/>
    public override LocationId LocationId { get; set; } = LocationId.CreateNew();

    /// <summary>
    /// Gets the destination variable name.
    /// </summary>
    required public IVariableName DestinationName { get; init; }

    /// <summary>
    /// Gets the source expression.
    /// </summary>
    required public Expression Expression { get; init; }
}
