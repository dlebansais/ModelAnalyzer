namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

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
    required public List<Invariant> InvariantList { get; init; }

    /// <summary>
    /// Gets unsupported class elements.
    /// </summary>
    required public Unsupported Unsupported { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public ManualResetEvent InvariantViolationVerified { get; } = new(initialState: false);

    /// <inheritdoc/>
    [JsonIgnore]
    public bool IsInvariantViolated { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder Builder = new();

        AppendClassName(Builder);
        AppendFields(Builder);
        AppendMethods(Builder);
        AppendInvariants(Builder);

        return TextBuilder.Normalized(Builder.ToString());
    }

    private void AppendClassName(StringBuilder builder)
    {
        builder.AppendLine(Name);
    }

    private void AppendFields(StringBuilder builder)
    {
        foreach (KeyValuePair<FieldName, Field> FieldEntry in FieldTable)
            builder.AppendLine($"  int {FieldEntry.Value.Name}");
    }

    private void AppendMethods(StringBuilder builder)
    {
        foreach (KeyValuePair<MethodName, Method> MethodEntry in MethodTable)
            MethodToString(builder, MethodEntry.Value);
    }

    private void MethodToString(StringBuilder builder, Method method)
    {
        string Parameters = string.Empty;

        foreach (KeyValuePair<ParameterName, Parameter> ParameterEntry in method.ParameterTable)
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
