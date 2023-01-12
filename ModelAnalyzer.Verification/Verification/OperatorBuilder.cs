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
    public static Dictionary<BinaryArithmeticOperator, Func<SolverContext, ArithExpr, ArithExpr, ArithExpr>> BinaryArithmetic { get; } = new()
    {
        { BinaryArithmeticOperator.Add, (SolverContext context, ArithExpr left, ArithExpr right) => context.CreateAddExpr(left, right) },
        { BinaryArithmeticOperator.Subtract, (SolverContext context, ArithExpr left, ArithExpr right) => context.CreateSubtractExpr(left, right) },
        { BinaryArithmeticOperator.Multiply, (SolverContext context, ArithExpr left, ArithExpr right) => context.CreateMultiplyExpr(left, right) },
        { BinaryArithmeticOperator.Divide, (SolverContext context, ArithExpr left, ArithExpr right) => context.CreateDivideExpr(left, right) },
    };

    /// <summary>
    /// Gets the expression builder associated to a unary arithmetic operator.
    /// </summary>
    public static Dictionary<UnaryArithmeticOperator, Func<SolverContext, ArithExpr, ArithExpr>> UnaryArithmetic { get; } = new()
    {
        { UnaryArithmeticOperator.Minus, (SolverContext context, ArithExpr operand) => context.CreateNegateExpr(operand) },
    };

    /// <summary>
    /// Gets the expression builder associated to a binary logical operator.
    /// </summary>
    public static Dictionary<BinaryLogicalOperator, Func<SolverContext, BoolExpr, BoolExpr, BoolExpr>> BinaryLogical { get; } = new()
    {
        { BinaryLogicalOperator.Or, (SolverContext context, BoolExpr left, BoolExpr right) => context.CreateOrExpr(left, right) },
        { BinaryLogicalOperator.And, (SolverContext context, BoolExpr left, BoolExpr right) => context.CreateAndExpr(left, right) },
    };

    /// <summary>
    /// Gets the expression builder associated to a unary logical operator.
    /// </summary>
    public static Dictionary<UnaryLogicalOperator, Func<SolverContext, BoolExpr, BoolExpr>> UnaryLogical { get; } = new()
    {
        { UnaryLogicalOperator.Not, (SolverContext context, BoolExpr operand) => context.CreateNotExpr(operand) },
    };

    /// <summary>
    /// Gets the expression builder associated to an equality operator.
    /// </summary>
    public static Dictionary<EqualityOperator, Func<SolverContext, Expr, Expr, BoolExpr>> Equality { get; } = new()
    {
        { EqualityOperator.Equal, (SolverContext context, Expr left, Expr right) => context.CreateEqualExpr(left, right) },
        { EqualityOperator.NotEqual, (SolverContext context, Expr left, Expr right) => context.CreateNotEqualExpr(left, right) },
    };

    /// <summary>
    /// Gets the expression builder associated to a comparison operator.
    /// </summary>
    public static Dictionary<ComparisonOperator, Func<SolverContext, ArithExpr, ArithExpr, BoolExpr>> Comparison { get; } = new()
    {
        { ComparisonOperator.GreaterThan, (SolverContext context, ArithExpr left, ArithExpr right) => context.CreateGreaterThanExpr(left, right) },
        { ComparisonOperator.GreaterThanOrEqual, (SolverContext context, ArithExpr left, ArithExpr right) => context.CreateGreaterThanEqualToExpr(left, right) },
        { ComparisonOperator.LessThan, (SolverContext context, ArithExpr left, ArithExpr right) => context.CreateLesserThanExpr(left, right) },
        { ComparisonOperator.LessThanOrEqual, (SolverContext context, ArithExpr left, ArithExpr right) => context.CreateLesserThanEqualToExpr(left, right) },
    };
}
