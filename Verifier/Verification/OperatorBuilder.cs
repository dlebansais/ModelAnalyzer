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
    /// Gets supported binary arithmetic operators.
    /// </summary>
    public static Dictionary<BinaryArithmeticOperator, Func<Context, ArithExpr, ArithExpr, ArithExpr>> BinaryArithmetic { get; } = new()
    {
        { BinaryArithmeticOperator.Add, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkAdd(left, right) },
        { BinaryArithmeticOperator.Subtract, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkSub(left, right) },
        { BinaryArithmeticOperator.Multiply, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkMul(left, right) },
        { BinaryArithmeticOperator.Divide, (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkDiv(left, right) },
    };

    /// <summary>
    /// Gets supported unary arithmetic operators.
    /// </summary>
    public static Dictionary<UnaryArithmeticOperator, Func<Context, ArithExpr, ArithExpr>> UnaryArithmetic { get; } = new()
    {
        { UnaryArithmeticOperator.Minus, (Context ctx, ArithExpr operand) => ctx.MkUnaryMinus(operand) },
    };

    /// <summary>
    /// Gets supported logical operators.
    /// </summary>
    public static Dictionary<LogicalOperator, Func<Context, BoolExpr, BoolExpr, BoolExpr>> Logical { get; } = new()
    {
        { LogicalOperator.Or, (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkOr(left, right) },
        { LogicalOperator.And, (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkAnd(left, right) },
    };

    /// <summary>
    /// Gets supported comparison operators.
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
