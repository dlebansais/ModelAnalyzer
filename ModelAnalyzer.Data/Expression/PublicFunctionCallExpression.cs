namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a call to a function as an expression.
/// </summary>
internal class PublicFunctionCallExpression : Expression, IFunctionCallExpression, IPublicCall
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType()
    {
        // Can return ExpressionType.Other if the expression doesn't match an existing function name.
        return ReturnType;
    }

    /// <inheritdoc/>
    required public List<IVariable> VariablePath { get; init; }

    /// <inheritdoc/>
    required public MethodName Name { get; init; }

    /// <summary>
    /// Gets the function return type.
    /// </summary>
    required public ExpressionType ReturnType { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location NameLocation { get; init; }

    /// <inheritdoc/>
    required public List<Argument> ArgumentList { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string PathString = string.Join(".", VariablePath.ConvertAll(item => item.Name.Text));
        string ArgumentString = string.Join(", ", ArgumentList);

        return $"{PathString}.{Name.Text}({ArgumentString})";
    }
}
