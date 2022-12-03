namespace DemoAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents unsupported components in a class.
/// </summary>
internal record Unsupported : IUnsupported
{
    /// <summary>
    /// Gets or sets a value indicating whether the class declaration is not supported.
    /// </summary>
    public bool InvalidDeclaration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the class contains unsupported members (other than fields and methods).
    /// </summary>
    public bool HasUnsupporteMember { get; set; }

    /// <summary>
    /// Gets the list of unsupported fields.
    /// </summary>
    public IReadOnlyList<IUnsupportedField> Fields => InternalFields.AsReadOnly();

    private List<IUnsupportedField> InternalFields = new();

    public void AddUnsupportedField(Location location, out IUnsupportedField newItem)
    {
        newItem = new UnsupportedField { Location = location };
        InternalFields.Add(newItem);
    }

    public IReadOnlyList<IUnsupportedMethod> Methods => InternalMethods.AsReadOnly();

    private List<IUnsupportedMethod> InternalMethods = new();

    public void AddUnsupportedMethod(Location location, out IUnsupportedMethod newItem)
    {
        newItem = new UnsupportedMethod { Location = location };
        InternalMethods.Add(newItem);
    }

    public IReadOnlyList<IUnsupportedParameter> Parameters => InternalParameters.AsReadOnly();

    private List<IUnsupportedParameter> InternalParameters = new();

    public void AddUnsupportedParameter(Location location, out IUnsupportedParameter newItem)
    {
        newItem = new UnsupportedParameter { Location = location };
        InternalParameters.Add(newItem);
    }

    public IReadOnlyList<IUnsupportedRequire> Requires => InternalRequires.AsReadOnly();

    private List<IUnsupportedRequire> InternalRequires = new();

    public void AddUnsupportedRequire(string text, Location location, out IUnsupportedRequire newItem)
    {
        newItem = new UnsupportedRequire { Text = text, Location = location };
        InternalRequires.Add(newItem);
    }

    public IReadOnlyList<IUnsupportedEnsure> Ensures => InternalEnsures.AsReadOnly();

    private List<IUnsupportedEnsure> InternalEnsures = new();

    public void AddUnsupportedEnsure(string text, Location location, out IUnsupportedEnsure newItem)
    {
        newItem = new UnsupportedEnsure { Text = text, Location = location };
        InternalEnsures.Add(newItem);
    }

    public IReadOnlyList<IUnsupportedStatement> Statements => InternalStatements.AsReadOnly();

    private List<IUnsupportedStatement> InternalStatements = new();

    public void AddUnsupportedStatement(Location location, out IUnsupportedStatement newItem)
    {
        newItem = new UnsupportedStatement { Location = location };
        InternalStatements.Add(newItem);
    }

    public IReadOnlyList<IUnsupportedExpression> Expressions => InternalExpressions.AsReadOnly();

    private List<IUnsupportedExpression> InternalExpressions = new();

    public void AddUnsupportedExpression(Location location, out IUnsupportedExpression newItem)
    {
        newItem = new UnsupportedExpression { Location = location };
        InternalExpressions.Add(newItem);
    }

    public IReadOnlyList<IUnsupportedInvariant> Invariants => InternalInvariants.AsReadOnly();

    private List<IUnsupportedInvariant> InternalInvariants = new();

    public void AddUnsupportedInvariant(string text, Location location, out IUnsupportedInvariant newItem)
    {
        newItem = new UnsupportedInvariant { Text = text, Location = location };
        InternalInvariants.Add(newItem);
    }

    public bool IsEmpty => !InvalidDeclaration &&
                           !HasUnsupporteMember &&
                           Fields.Count == 0 &&
                           Methods.Count == 0 &&
                           Parameters.Count == 0 &&
                           Requires.Count == 0 &&
                           Ensures.Count == 0 &&
                           Statements.Count == 0 &&
                           Expressions.Count == 0;
}
