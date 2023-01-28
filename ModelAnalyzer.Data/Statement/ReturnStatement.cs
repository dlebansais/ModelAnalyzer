namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a return statement.
/// </summary>
[DebuggerDisplay("return {Expression}")]
internal class ReturnStatement : Statement
{
    /// <inheritdoc/>
    public override LocationId LocationId { get; set; } = LocationId.CreateNew();

    /// <summary>
    /// Gets the expression giving the returned value.
    /// </summary>
    required public IExpression? Expression { get; init; }
}
