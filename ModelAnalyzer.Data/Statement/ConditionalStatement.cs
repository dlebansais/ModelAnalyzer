﻿namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a conditional statement.
/// </summary>
[DebuggerDisplay("if ({Condition}) then ...")]
internal class ConditionalStatement : Statement
{
    /// <inheritdoc/>
    public override LocationId LocationId { get; set; } = LocationId.CreateNew();

    /// <summary>
    /// Gets the condition.
    /// </summary>
    required public Expression Condition { get; init; }

    /// <summary>
    /// Gets statements when the condition is true.
    /// </summary>
    required public BlockScope WhenTrueBlock { get; init; }

    /// <summary>
    /// Gets statements when the condition is false.
    /// </summary>
    required public BlockScope WhenFalseBlock { get; init; }
}
