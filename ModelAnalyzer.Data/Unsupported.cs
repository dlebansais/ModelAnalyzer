namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents unsupported components in a class.
/// </summary>
public record Unsupported : IUnsupported
{
    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool InvalidDeclaration { get; set; }

    /// <inheritdoc/>
    public bool HasUnsupporteMember { get; set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public IReadOnlyList<IUnsupportedField> Fields => InternalFields.AsReadOnly();

    /// <summary>
    /// Adds an unsupported field.
    /// </summary>
    /// <param name="location">The field location.</param>
    public void AddUnsupportedField(Location location)
    {
        UnsupportedField NewItem = new UnsupportedField { Location = location };
        InternalFields.Add(NewItem);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public IReadOnlyList<IUnsupportedMethod> Methods => InternalMethods.AsReadOnly();

    /// <summary>
    /// Adds an unsupported field.
    /// </summary>
    /// <param name="location">The method location.</param>
    public void AddUnsupportedMethod(Location location)
    {
        UnsupportedMethod NewItem = new UnsupportedMethod { Location = location };
        InternalMethods.Add(NewItem);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public IReadOnlyList<IUnsupportedParameter> Parameters => InternalParameters.AsReadOnly();

    /// <summary>
    /// Adds an unsupported parameter.
    /// </summary>
    /// <param name="location">The parameter location.</param>
    public void AddUnsupportedParameter(Location location)
    {
        UnsupportedParameter NewItem = new UnsupportedParameter { Location = location };
        InternalParameters.Add(NewItem);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public IReadOnlyList<IUnsupportedRequire> Requires => InternalRequires.AsReadOnly();

    /// <summary>
    /// Adds an unsupported requirement.
    /// </summary>
    /// <param name="text">The requirement text.</param>
    /// <param name="location">The requirement location.</param>
    public void AddUnsupportedRequire(string text, Location location)
    {
        UnsupportedRequire NewItem = new UnsupportedRequire { Text = text, Location = location };
        InternalRequires.Add(NewItem);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public IReadOnlyList<IUnsupportedEnsure> Ensures => InternalEnsures.AsReadOnly();

    /// <summary>
    /// Adds an unsupported guarantee.
    /// </summary>
    /// <param name="text">The guarantee text.</param>
    /// <param name="location">The guarantee location.</param>
    public void AddUnsupportedEnsure(string text, Location location)
    {
        UnsupportedEnsure NewItem = new UnsupportedEnsure { Text = text, Location = location };
        InternalEnsures.Add(NewItem);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public IReadOnlyList<IUnsupportedStatement> Statements => InternalStatements.AsReadOnly();

    /// <summary>
    /// Adds an unsupported statement.
    /// </summary>
    /// <param name="location">The statement location.</param>
    public void AddUnsupportedStatement(Location location)
    {
        UnsupportedStatement NewItem = new UnsupportedStatement { Location = location };
        InternalStatements.Add(NewItem);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public IReadOnlyList<IUnsupportedExpression> Expressions => InternalExpressions.AsReadOnly();

    /// <summary>
    /// Adds an unsupported expression.
    /// </summary>
    /// <param name="location">The expression location.</param>
    public void AddUnsupportedExpression(Location location)
    {
        UnsupportedExpression NewItem = new UnsupportedExpression { Location = location };
        InternalExpressions.Add(NewItem);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public IReadOnlyList<IUnsupportedInvariant> Invariants => InternalInvariants.AsReadOnly();

    /// <summary>
    /// Adds an unsupported invariant.
    /// </summary>
    /// <param name="text">The invariant text.</param>
    /// <param name="location">The invariant location.</param>
    public void AddUnsupportedInvariant(string text, Location location)
    {
        UnsupportedInvariant NewItem = new UnsupportedInvariant { Text = text, Location = location };
        InternalInvariants.Add(NewItem);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<UnsupportedField> InternalFields { get; set; } = new();
    public List<UnsupportedMethod> InternalMethods { get; set; } = new();
    public List<UnsupportedParameter> InternalParameters { get; set; } = new();
    public List<UnsupportedRequire> InternalRequires { get; set; } = new();
    public List<UnsupportedEnsure> InternalEnsures { get; set; } = new();
    public List<UnsupportedStatement> InternalStatements { get; set; } = new();
    public List<UnsupportedExpression> InternalExpressions { get; set; } = new();
    public List<UnsupportedInvariant> InternalInvariants { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
