﻿namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a call to a function as an expression.
/// </summary>
internal class PrivateFunctionCallExpression : Expression
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

    /// <summary>
    /// Gets the function name.
    /// </summary>
    required public MethodName FunctionName { get; init; }

    /// <summary>
    /// Gets the function return type.
    /// </summary>
    required public ExpressionType ReturnType { get; init; }

    /// <summary>
    /// Gets the function name location.
    /// </summary>
    [JsonIgnore]
    required public Location NameLocation { get; init; }

    /// <summary>
    /// Gets the list of arguments.
    /// </summary>
    required public List<Argument> ArgumentList { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string ArgumentString = string.Join(", ", ArgumentList);

        return $"{FunctionName.Text}({ArgumentString})";
    }
}