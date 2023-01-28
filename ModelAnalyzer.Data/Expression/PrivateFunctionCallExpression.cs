﻿namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a call to a function as an expression.
/// </summary>
internal class PrivateFunctionCallExpression : Expression, IFunctionCallExpression, IPrivateCall
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
    public override LocationId LocationId { get; set; } = LocationId.CreateNew();

    /// <inheritdoc/>
    required public ClassName ClassName { get; init; }

    /// <inheritdoc/>
    required public MethodName MethodName { get; init; }

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
    required public ClassName CallerClassName { get; init; }

    /// <inheritdoc/>
    public bool IsStatic { get => ClassName != ClassName.Empty; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string StaticString = ClassName == ClassName.Empty ? string.Empty : $"{ClassName}.";
        string ArgumentString = string.Join(", ", ArgumentList);

        return $"{StaticString}{MethodName.Text}({ArgumentString})";
    }
}
