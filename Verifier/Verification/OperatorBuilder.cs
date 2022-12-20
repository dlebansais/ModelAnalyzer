namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using Microsoft.Z3;

/// <summary>
/// Provides tables of supported operators.
/// </summary>
internal static class OperatorBuilder
{
    /// <summary>
    /// Gets the expression builder associated to a binary arithmetic operator.
    /// </summary>
    public static Dictionary<BinaryArithmeticOperator, Func<Context, ArithExpr, ArithExpr, ArithExpr>> BinaryArithmetic { get; } = new()
    {
        { BinaryArithmeticOperator.Add, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkAdd(left, right) },
        { BinaryArithmeticOperator.Subtract, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkSub(left, right) },
        { BinaryArithmeticOperator.Multiply, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkMul(left, right) },
        { BinaryArithmeticOperator.Divide, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkDiv(left, right) },
    };

    /// <summary>
    /// Gets the expression builder associated to a unary arithmetic operator.
    /// </summary>
    public static Dictionary<UnaryArithmeticOperator, Func<Context, ArithExpr, ArithExpr>> UnaryArithmetic { get; } = new()
    {
        { UnaryArithmeticOperator.Minus, (Context ctx, ArithExpr operand) => ctx.MkUnaryMinus(operand) },
    };

    /// <summary>
    /// Gets the expression builder associated to a binary logical operator.
    /// </summary>
    public static Dictionary<BinaryLogicalOperator, Func<Context, BoolExpr, BoolExpr, BoolExpr>> BinaryLogical { get; } = new()
    {
        { BinaryLogicalOperator.Or, (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkOr(left, right) },
        { BinaryLogicalOperator.And, (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkAnd(left, right) },
    };

    /// <summary>
    /// Gets the expression builder associated to a unary logical operator.
    /// </summary>
    public static Dictionary<UnaryLogicalOperator, Func<Context, BoolExpr, BoolExpr>> UnaryLogical { get; } = new()
    {
        { UnaryLogicalOperator.Not, (Context ctx, BoolExpr operand) => ctx.MkNot(operand) },
    };

    /// <summary>
    /// Gets the expression builder associated to an equality operator.
    /// </summary>
    public static Dictionary<EqualityOperator, Func<Context, Expr, Expr, BoolExpr>> Equality { get; } = new()
    {
        { EqualityOperator.Equal, (Context ctx, Expr left, Expr right) => ctx.MkEq(left, right) },
        { EqualityOperator.NotEqual, (Context ctx, Expr left, Expr right) => ctx.MkNot(ctx.MkEq(left, right)) },
    };

    /// <summary>
    /// Gets the expression builder associated to a comparison operator.
    /// </summary>
    public static Dictionary<ComparisonOperator, Func<Context, ArithExpr, ArithExpr, BoolExpr>> Comparison { get; } = new()
    {
        { ComparisonOperator.GreaterThan, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkGt(left, right) },
        { ComparisonOperator.GreaterThanOrEqual, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkGe(left, right) },
        { ComparisonOperator.LessThan, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkLt(left, right) },
        { ComparisonOperator.LessThanOrEqual, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkLe(left, right) },
    };
}
