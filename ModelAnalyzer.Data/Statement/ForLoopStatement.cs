namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a conditional statement.
/// </summary>
[DebuggerDisplay("for (int {LocalIndex.Name.Text} = {LocalIndex.Initializer}; {ContinueCondition}; {LocalIndex.Name.Text}++) ...")]
internal class ForLoopStatement : Statement
{
    /// <inheritdoc/>
    public override LocationId LocationId { get; set; } = LocationId.CreateNew();

    /// <summary>
    /// Gets the index.
    /// </summary>
    required public Local LocalIndex { get; init; }

    /// <summary>
    /// Gets the continue condition.
    /// </summary>
    required public Expression ContinueCondition { get; init; }

    /// <summary>
    /// Gets the loop statements.
    /// </summary>
    required public BlockScope Block { get; init; }
}
