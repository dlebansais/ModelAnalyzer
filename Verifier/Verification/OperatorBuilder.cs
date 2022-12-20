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
    /// Gets the table associating binary arithmetic operators to an expression builder.
    /// </summary>
    public static Dictionary<BinaryArithmeticOperator, Func<Context, ArithExpr, ArithExpr, ArithExpr>> BinaryArithmetic { get; } = new()
    {
        { BinaryArithmeticOperator.Add, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkAdd(left, right) },
        { BinaryArithmeticOperator.Subtract, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkSub(left, right) },
        { BinaryArithmeticOperator.Multiply, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkMul(left, right) },
        { BinaryArithmeticOperator.Divide, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkDiv(left, right) },
    };

    /// <summary>
    /// Gets the table associating unary arithmetic operators to an expression builder.
    /// </summary>
    public static Dictionary<UnaryArithmeticOperator, Func<Context, ArithExpr, ArithExpr>> UnaryArithmetic { get; } = new()
    {
        { UnaryArithmeticOperator.Minus, (Context ctx, ArithExpr operand) => ctx.MkUnaryMinus(operand) },
    };

    /// <summary>
    /// Gets the table associating binary logical operators to an expression builder.
    /// </summary>
    public static Dictionary<BinaryLogicalOperator, Func<Context, BoolExpr, BoolExpr, BoolExpr>> BinaryLogical { get; } = new()
    {
        { BinaryLogicalOperator.Or, (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkOr(left, right) },
        { BinaryLogicalOperator.And, (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkAnd(left, right) },
    };

    /// <summary>
    /// Gets the table associating unary logical operators to an expression builder.
    /// </summary>
    public static Dictionary<UnaryLogicalOperator, Func<Context, BoolExpr, BoolExpr>> UnaryLogical { get; } = new()
    {
        { UnaryLogicalOperator.Not, (Context ctx, BoolExpr operand) => ctx.MkNot(operand) },
    };

    /// <summary>
    /// Gets the table associating comparison operators to an expression builder.
    /// </summary>
    public static Dictionary<ComparisonOperator, Func<Context, ArithExpr, ArithExpr, BoolExpr>> Comparison { get; } = new()
    {
        { ComparisonOperator.Equal, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkEq(left, right) },
        { ComparisonOperator.NotEqual, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkNot(ctx.MkEq(left, right)) },
        { ComparisonOperator.GreaterThan, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkGt(left, right) },
        { ComparisonOperator.GreaterThanOrEqual, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkGe(left, right) },
        { ComparisonOperator.LessThan, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkLt(left, right) },
        { ComparisonOperator.LessThanOrEqual, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkLe(left, right) },
    };
}
