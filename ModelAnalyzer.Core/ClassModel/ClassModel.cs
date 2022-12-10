namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Text;
using Microsoft.Z3;

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
        StringBuilder Builder = new();

        AppendClassName(Builder);
        AppendFields(Builder);
        AppendMethods(Builder);
        AppendInvariants(Builder);

        return Builder.ToString();
    }

    private void AppendClassName(StringBuilder builder)
    {
        builder.AppendLine(Name);
    }

    private void AppendFields(StringBuilder builder)
    {
        foreach (KeyValuePair<FieldName, IField> FieldEntry in FieldTable)
            if (FieldEntry.Value is Field Field)
                builder.AppendLine($"  int {Field.Name}");
    }

    private void AppendMethods(StringBuilder builder)
    {
        foreach (KeyValuePair<MethodName, IMethod> MethodEntry in MethodTable)
            if (MethodEntry.Value is Method Method)
                MethodToString(builder, Method);
    }

    private void MethodToString(StringBuilder builder, Method method)
    {
        string Parameters = string.Empty;

        foreach (KeyValuePair<ParameterName, IParameter> ParameterEntry in method.ParameterTable)
        {
            if (Parameters.Length > 0)
                Parameters += ", ";

            Parameters += ParameterEntry.Key.Name;
        }

        string ReturnString = method.HasReturnValue ? "int" : "void";
        builder.AppendLine($"  {ReturnString} {method.MethodName.Name}({Parameters})");

        foreach (IRequire Item in method.RequireList)
            if (Item is Require Require && Require.BooleanExpression is Expression BooleanExpression)
                AppendAssertion(builder, "require", BooleanExpression);

        foreach (IEnsure Item in method.EnsureList)
            if (Item is Ensure Ensure && Ensure.BooleanExpression is Expression BooleanExpression)
                AppendAssertion(builder, "ensure", BooleanExpression);
    }

    private void AppendAssertion(StringBuilder builder, string text, Expression booleanExpression)
    {
        builder.AppendLine($"    {text} {booleanExpression.ToSimpleString()}");
    }

    private void AppendInvariants(StringBuilder builder)
    {
        foreach (Invariant Invariant in InvariantList)
            builder.AppendLine($"  * {Invariant.BooleanExpression}");
    }
}
