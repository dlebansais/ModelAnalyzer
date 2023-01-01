namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

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
    public IReadOnlyList<IField> GetFields()
    {
        List<IField> Result = new();

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
            Result.Add(Entry.Value);

        return Result.AsReadOnly();
    }

    /// <summary>
    /// Gets the method table.
    /// </summary>
    required public ReadOnlyMethodTable MethodTable { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<IMethod> GetMethods()
    {
        List<IMethod> Result = new();

        foreach (KeyValuePair<MethodName, Method> Entry in MethodTable)
            Result.Add(Entry.Value);

        return Result.AsReadOnly();
    }

    /// <summary>
    /// Gets the list of invariants.
    /// </summary>
    required public IReadOnlyList<Invariant> InvariantList { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<IInvariant> GetInvariants()
    {
        List<IInvariant> Result = new();

        foreach (Invariant Item in InvariantList)
            Result.Add(Item);

        return Result.AsReadOnly();
    }

    /// <inheritdoc/>
    required public Unsupported Unsupported { get; init; }

    /// <inheritdoc/>
    required public IReadOnlyList<IInvariantViolation> InvariantViolations { get; init; }

    /// <inheritdoc/>
    required public IReadOnlyList<IRequireViolation> RequireViolations { get; init; }

    /// <inheritdoc/>
    required public IReadOnlyList<IEnsureViolation> EnsureViolations { get; init; }
}
