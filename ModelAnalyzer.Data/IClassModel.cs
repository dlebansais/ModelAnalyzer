namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides information about a class model.
/// </summary>
public interface IClassModel
{
    /// <summary>
    /// Gets the class name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets unsupported class elements.
    /// </summary>
    Unsupported Unsupported { get; }

    /// <summary>
    /// Gets the list of violated invariants.
    /// </summary>
    IReadOnlyList<IInvariantViolation> InvariantViolations { get; }

    /// <summary>
    /// Gets the list of violated require clauses.
    /// </summary>
    IReadOnlyList<IRequireViolation> RequireViolations { get; }

    /// <summary>
    /// Gets the list of violated ensure clauses.
    /// </summary>
    IReadOnlyList<IEnsureViolation> EnsureViolations { get; }
}
