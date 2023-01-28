namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

/// <summary>
/// Provides information about a requirement violation.
/// </summary>
public interface IRequireViolation : IAssertionViolation
{
    /// <summary>
    /// Gets the method where the require clause is found.
    /// </summary>
    IMethod Method { get; }

    /// <summary>
    /// Gets the assertion text.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Gets the function or method name location.
    /// </summary>
    Location NameLocation { get; }

    /// <summary>
    /// Gets the statement call that triggered the require clause.
    /// </summary>
    IStatement? Statement { get; }

    /// <summary>
    /// Gets the function call that triggered the require clause.
    /// </summary>
    IExpression? Expression { get; }

    /// <summary>
    /// Gets the require clause.
    /// </summary>
    IRequire? Require { get; }
}
