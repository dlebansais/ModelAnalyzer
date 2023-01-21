﻿namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a call to a function as an expression.
/// </summary>
internal class PrivateFunctionCallExpression : Expression, IFunctionCallExpression
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
    required public ClassModel? ClassModel { get; init; }

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
        string StaticString = ClassModel is null ? string.Empty : $"{ClassModel.Name}.";
        string ArgumentString = string.Join(", ", ArgumentList);

        return $"{StaticString}{Name.Text}({ArgumentString})";
    }
}
