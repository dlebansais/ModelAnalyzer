﻿namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents an assignment statement.
/// </summary>
[DebuggerDisplay("{DestinationName.Text, nq}{(DestinationIndex is null ? \"\" : $\"[{DestinationIndex}]\"), nq} = {Expression}")]
internal class AssignmentStatement : Statement
{
    /// <inheritdoc/>
    public override LocationId LocationId { get; set; } = LocationId.CreateNew();

    /// <summary>
    /// Gets the destination variable name.
    /// </summary>
    required public IVariableName DestinationName { get; init; }

    /// <summary>
    /// Gets the destination index for an array.
    /// </summary>
    required public Expression? DestinationIndex { get; init; }

    /// <summary>
    /// Gets the source expression.
    /// </summary>
    required public Expression Expression { get; init; }
}
