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
    /// Gets the list of fields.
    /// </summary>
    IList<IField> GetFields();

    /// <summary>
    /// Gets the list of methods.
    /// </summary>
    IList<IMethod> GetMethods();

    /// <summary>
    /// Gets the list of invariants.
    /// </summary>
    IList<IInvariant> GetInvariants();

    /// <summary>
    /// Gets unsupported class elements.
    /// </summary>
    IUnsupported Unsupported { get; }

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

    /// <summary>
    /// Gets the list of violated flow checks.
    /// </summary>
    IReadOnlyList<IAssumeViolation> AssumeViolations { get; }
}
