namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents unsupported components in a class.
/// </summary>
internal record Unsupported : IUnsupported
{
    /// <inheritdoc/>
    public bool IsEmpty => !InvalidDeclaration &&
                           !HasUnsupporteMember &&
                           !IsPartOfCycle &&
                           InternalProperties.Count == 0 &&
                           InternalFields.Count == 0 &&
                           InternalMethods.Count == 0 &&
                           InternalParameters.Count == 0 &&
                           InternalRequires.Count == 0 &&
                           InternalEnsures.Count == 0 &&
                           InternalLocals.Count == 0 &&
                           InternalStatements.Count == 0 &&
                           InternalExpressions.Count == 0 &&
                           InternalInvariants.Count == 0;

    /// <inheritdoc/>
    public bool InvalidDeclaration { get; set; }

    /// <inheritdoc/>
    public bool HasUnsupporteMember { get; set; }

    /// <inheritdoc/>
    public bool IsPartOfCycle { get; set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public IReadOnlyList<IUnsupportedProperty> Properties => InternalProperties.AsReadOnly();

    /// <summary>
    /// Adds an unsupported property.
    /// </summary>
    /// <param name="location">The property location.</param>
    public void AddUnsupportedProperty(Location location)
    {
        UnsupportedProperty NewItem = new UnsupportedProperty { Location = location };
        InternalProperties.Add(NewItem);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public IReadOnlyList<IUnsupportedField> Fields => InternalFields.AsReadOnly();

    /// <summary>
    /// Adds an unsupported field.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="location">The field location.</param>
    public void AddUnsupportedField(FieldName name, Location location)
    {
        UnsupportedField NewItem = new UnsupportedField { Name = name, Location = location };
        InternalFields.Add(NewItem);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public IReadOnlyList<IUnsupportedMethod> Methods => InternalMethods.AsReadOnly();

    /// <summary>
    /// Adds an unsupported method.
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
    public IReadOnlyList<IUnsupportedLocal> Locals => InternalLocals.AsReadOnly();

    /// <summary>
    /// Adds an unsupported local.
    /// </summary>
    /// <param name="location">The local location.</param>
    public void AddUnsupportedLocal(Location location)
    {
        UnsupportedLocal NewItem = new UnsupportedLocal { Location = location };
        InternalLocals.Add(NewItem);
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
    public List<UnsupportedProperty> InternalProperties { get; set; } = new();
    public List<UnsupportedField> InternalFields { get; set; } = new();
    public List<UnsupportedMethod> InternalMethods { get; set; } = new();
    public List<UnsupportedParameter> InternalParameters { get; set; } = new();
    public List<UnsupportedRequire> InternalRequires { get; set; } = new();
    public List<UnsupportedEnsure> InternalEnsures { get; set; } = new();
    public List<UnsupportedLocal> InternalLocals { get; set; } = new();
    public List<UnsupportedStatement> InternalStatements { get; set; } = new();
    public List<UnsupportedExpression> InternalExpressions { get; set; } = new();
    public List<UnsupportedInvariant> InternalInvariants { get; set; } = new();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
