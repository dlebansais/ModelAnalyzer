namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents a block of statements with their own scope.
/// </summary>
internal record BlockScope
{
    /// <summary>
    /// Gets the block local variables.
    /// </summary>
    required public ReadOnlyLocalTable LocalTable { get; init; }

    /// <summary>
    /// Gets the index local variable if this is a for loop block.
    /// </summary>
    required public Local? IndexLocal { get; init; }

    /// <summary>
    /// Gets the block statements.
    /// </summary>
    required public List<Statement> StatementList { get; init; }
}
