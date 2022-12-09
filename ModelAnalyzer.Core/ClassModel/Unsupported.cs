namespace ModelAnalyzer;

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

    /// <summary>
    /// Adds an unsupported field.
    /// </summary>
    /// <param name="location">The field location.</param>
    /// <param name="newItem">The added field upon return.</param>
    public void AddUnsupportedField(Location location, out IUnsupportedField newItem)
    {
        newItem = new UnsupportedField { Location = location };
        InternalFields.Add(newItem);
    }

    /// <summary>
    /// Gets the list of unsupported methods.
    /// </summary>
    public IReadOnlyList<IUnsupportedMethod> Methods => InternalMethods.AsReadOnly();

    /// <summary>
    /// Adds an unsupported field.
    /// </summary>
    /// <param name="location">The method location.</param>
    /// <param name="newItem">The added method upon return.</param>
    public void AddUnsupportedMethod(Location location, out IUnsupportedMethod newItem)
    {
        newItem = new UnsupportedMethod { Location = location };
        InternalMethods.Add(newItem);
    }

    /// <summary>
    /// Gets the list of unsupported parameters.
    /// </summary>
    public IReadOnlyList<IUnsupportedParameter> Parameters => InternalParameters.AsReadOnly();

    /// <summary>
    /// Adds an unsupported parameter.
    /// </summary>
    /// <param name="location">The parameter location.</param>
    /// <param name="newItem">The added parameter upon return.</param>
    public void AddUnsupportedParameter(Location location, out IUnsupportedParameter newItem)
    {
        newItem = new UnsupportedParameter { Location = location };
        InternalParameters.Add(newItem);
    }

    /// <summary>
    /// Gets the list of unsupported requirements.
    /// </summary>
    public IReadOnlyList<IUnsupportedRequire> Requires => InternalRequires.AsReadOnly();

    /// <summary>
    /// Adds an unsupported requirement.
    /// </summary>
    /// <param name="text">The requirement text.</param>
    /// <param name="location">The requirement location.</param>
    /// <param name="newItem">The added requirement upon return.</param>
    public void AddUnsupportedRequire(string text, Location location, out IUnsupportedRequire newItem)
    {
        newItem = new UnsupportedRequire { Text = text, Location = location };
        InternalRequires.Add(newItem);
    }

    /// <summary>
    /// Gets the list of unsupported guarantees.
    /// </summary>
    public IReadOnlyList<IUnsupportedEnsure> Ensures => InternalEnsures.AsReadOnly();

    /// <summary>
    /// Adds an unsupported guarantee.
    /// </summary>
    /// <param name="text">The guarantee text.</param>
    /// <param name="location">The guarantee location.</param>
    /// <param name="newItem">The added guarantee upon return.</param>
    public void AddUnsupportedEnsure(string text, Location location, out IUnsupportedEnsure newItem)
    {
        newItem = new UnsupportedEnsure { Text = text, Location = location };
        InternalEnsures.Add(newItem);
    }

    /// <summary>
    /// Gets the list of unsupported statements.
    /// </summary>
    public IReadOnlyList<IUnsupportedStatement> Statements => InternalStatements.AsReadOnly();

    /// <summary>
    /// Adds an unsupported statement.
    /// </summary>
    /// <param name="location">The statement location.</param>
    /// <param name="newItem">The added statement upon return.</param>
    public void AddUnsupportedStatement(Location location, out IUnsupportedStatement newItem)
    {
        newItem = new UnsupportedStatement { Location = location };
        InternalStatements.Add(newItem);
    }

    /// <summary>
    /// Gets the list of unsupported expressions.
    /// </summary>
    public IReadOnlyList<IUnsupportedExpression> Expressions => InternalExpressions.AsReadOnly();

    /// <summary>
    /// Adds an unsupported expression.
    /// </summary>
    /// <param name="location">The expression location.</param>
    /// <param name="newItem">The added expression upon return.</param>
    public void AddUnsupportedExpression(Location location, out IUnsupportedExpression newItem)
    {
        newItem = new UnsupportedExpression { Location = location };
        InternalExpressions.Add(newItem);
    }

    /// <summary>
    /// Gets the list of unsupported invariants.
    /// </summary>
    public IReadOnlyList<IUnsupportedInvariant> Invariants => InternalInvariants.AsReadOnly();

    /// <summary>
    /// Adds an unsupported invariant.
    /// </summary>
    /// <param name="text">The invariant text.</param>
    /// <param name="location">The invariant location.</param>
    /// <param name="newItem">The added invariant upon return.</param>
    public void AddUnsupportedInvariant(string text, Location location, out IUnsupportedInvariant newItem)
    {
        newItem = new UnsupportedInvariant { Text = text, Location = location };
        InternalInvariants.Add(newItem);
    }

    /// <summary>
    /// Gets a value indicating whether there is nothing unsupported.
    /// </summary>
    public bool IsEmpty => !InvalidDeclaration &&
                           !HasUnsupporteMember &&
                           Fields.Count == 0 &&
                           Methods.Count == 0 &&
                           Parameters.Count == 0 &&
                           Requires.Count == 0 &&
                           Ensures.Count == 0 &&
                           Statements.Count == 0 &&
                           Expressions.Count == 0 &&
                           Invariants.Count == 0;

    private List<IUnsupportedField> InternalFields = new();
    private List<IUnsupportedMethod> InternalMethods = new();
    private List<IUnsupportedParameter> InternalParameters = new();
    private List<IUnsupportedRequire> InternalRequires = new();
    private List<IUnsupportedEnsure> InternalEnsures = new();
    private List<IUnsupportedStatement> InternalStatements = new();
    private List<IUnsupportedExpression> InternalExpressions = new();
    private List<IUnsupportedInvariant> InternalInvariants = new();
}
