namespace DemoAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides information about unsupported features of a class model.
/// </summary>
public interface IUnsupported
{
    /// <summary>
    /// Gets a value indicating whether there is any unsupported feature.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Gets the list of unsupported fields.
    /// </summary>
    IReadOnlyList<IUnsupportedField> Fields { get; }

    /// <summary>
    /// Gets the list of unsupported methods.
    /// </summary>
    IReadOnlyList<IUnsupportedMethod> Methods { get; }

    /// <summary>
    /// Gets the list of unsupported parameters.
    /// </summary>
    IReadOnlyList<IUnsupportedParameter> Parameters { get; }

    /// <summary>
    /// Gets the list of unsupported requirement assertions.
    /// </summary>
    IReadOnlyList<IUnsupportedRequire> Requires { get; }

    /// <summary>
    /// Gets the list of unsupported guarantee assertions.
    /// </summary>
    IReadOnlyList<IUnsupportedEnsure> Ensures { get; }

    /// <summary>
    /// Gets the list of unsupported statements.
    /// </summary>
    IReadOnlyList<IUnsupportedStatement> Statements { get; }

    /// <summary>
    /// Gets the list of unsupported expressions.
    /// </summary>
    IReadOnlyList<IUnsupportedExpression> Expressions { get; }

    /// <summary>
    /// Gets the list of unsupported invariants.
    /// </summary>
    IReadOnlyList<IUnsupportedInvariant> Invariants { get; }
}
