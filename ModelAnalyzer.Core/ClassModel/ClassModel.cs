namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents the model of a class.
/// </summary>
internal partial record ClassModel : IClassModel
{
    /// <summary>
    /// Gets the class name.
    /// </summary>
    required public string Name { get; init; }

    /// <summary>
    /// Gets the class manager.
    /// </summary>
    required public ClassModelManager Manager { get; init; }

    /// <summary>
    /// Gets the field table.
    /// </summary>
    required public FieldTable FieldTable { get; init; }

    /// <summary>
    /// Gets the method table.
    /// </summary>
    required public MethodTable MethodTable { get; init; }

    /// <summary>
    /// Gets the list of invariants.
    /// </summary>
    required public List<IInvariant> InvariantList { get; init; }

    /// <summary>
    /// Gets unsupported class elements.
    /// </summary>
    required public IUnsupported Unsupported { get; init; }

    /// <inheritdoc/>
    public bool IsInvariantViolated { get; internal set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string Result = @$"{Name}
";

        foreach (KeyValuePair<FieldName, IField> FieldEntry in FieldTable)
            if (FieldEntry.Value is Field Field)
                Result += @$"  int {Field.Name}
";

        foreach (KeyValuePair<MethodName, IMethod> MethodEntry in MethodTable)
            if (MethodEntry.Value is Method Method)
            {
                string Parameters = string.Empty;

                foreach (KeyValuePair<ParameterName, IParameter> ParameterEntry in Method.ParameterTable)
                {
                    if (Parameters.Length > 0)
                        Parameters += ", ";

                    Parameters += ParameterEntry.Key.Name;
                }

                string ReturnString = Method.HasReturnValue ? "int" : "void";
                Result += @$"  {ReturnString} {Method.MethodName.Name}({Parameters})
";

                foreach (Invariant Invariant in InvariantList)
                {
                    Result += @$"  * {Invariant.BooleanExpression}
";
                }
            }

        return Result;
    }
}
