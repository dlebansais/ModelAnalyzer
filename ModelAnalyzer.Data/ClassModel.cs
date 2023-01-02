namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents the model of a class.
/// </summary>
internal partial record ClassModel : IClassModel
{
    /// <inheritdoc/>
    required public string Name { get; init; }

    /// <summary>
    /// Gets the field table.
    /// </summary>
    required public ReadOnlyFieldTable FieldTable { get; init; }

    /// <inheritdoc/>
    public IList<IField> GetFields()
    {
        List<IField> Result = new();

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
            Result.Add(Entry.Value);

        return Result;
    }

    /// <summary>
    /// Gets the method table.
    /// </summary>
    required public ReadOnlyMethodTable MethodTable { get; init; }

    /// <inheritdoc/>
    public IList<IMethod> GetMethods()
    {
        List<IMethod> Result = new();

        foreach (KeyValuePair<MethodName, Method> Entry in MethodTable)
            Result.Add(Entry.Value);

        return Result;
    }

    /// <summary>
    /// Gets the list of invariants.
    /// </summary>
    required public IReadOnlyList<Invariant> InvariantList { get; init; }

    /// <inheritdoc/>
    public IList<IInvariant> GetInvariants()
    {
        List<IInvariant> Result = new();

        foreach (Invariant Item in InvariantList)
            Result.Add(Item);

        return Result;
    }

    /// <summary>
    /// Gets unsupported class elements.
    /// </summary>
    required public Unsupported Unsupported { get; init; }

    /// <inheritdoc/>
    IUnsupported IClassModel.Unsupported { get => Unsupported; }

    /// <inheritdoc/>
    required public IReadOnlyList<IInvariantViolation> InvariantViolations { get; init; }

    /// <inheritdoc/>
    required public IReadOnlyList<IRequireViolation> RequireViolations { get; init; }

    /// <inheritdoc/>
    required public IReadOnlyList<IEnsureViolation> EnsureViolations { get; init; }
}
