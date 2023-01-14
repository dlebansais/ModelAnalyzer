namespace ModelAnalyzer;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides tables of supported operators.
/// </summary>
internal static class OperatorBuilder
{
    /// <summary>
    /// Gets the expression builder associated to a binary arithmetic operator.
    /// </summary>
    public static Dictionary<BinaryArithmeticOperator, Func<SolverContext, IArithExprCapsule, IArithExprCapsule, IArithExprCapsule>> BinaryArithmetic { get; } = new()
    {
        { BinaryArithmeticOperator.Add, (SolverContext context, IArithExprCapsule left, IArithExprCapsule right) => context.CreateAddExpr(left.Item, right.Item) },
        { BinaryArithmeticOperator.Subtract, (SolverContext context, IArithExprCapsule left, IArithExprCapsule right) => context.CreateSubtractExpr(left.Item, right.Item) },
        { BinaryArithmeticOperator.Multiply, (SolverContext context, IArithExprCapsule left, IArithExprCapsule right) => context.CreateMultiplyExpr(left.Item, right.Item) },
        { BinaryArithmeticOperator.Divide, (SolverContext context, IArithExprCapsule left, IArithExprCapsule right) => context.CreateDivideExpr(left.Item, right.Item) },
    };

    /// <summary>
    /// Gets the expression builder associated to a unary arithmetic operator.
    /// </summary>
    public static Dictionary<UnaryArithmeticOperator, Func<SolverContext, IArithExprCapsule, IArithExprCapsule>> UnaryArithmetic { get; } = new()
    {
        { UnaryArithmeticOperator.Minus, (SolverContext context, IArithExprCapsule operand) => context.CreateNegateExpr(operand.Item) },
    };

    /// <summary>
    /// Gets the expression builder associated to a binary logical operator.
    /// </summary>
    public static Dictionary<BinaryLogicalOperator, Func<SolverContext, IBoolExprCapsule, IBoolExprCapsule, IBoolExprCapsule>> BinaryLogical { get; } = new()
    {
        { BinaryLogicalOperator.Or, (SolverContext context, IBoolExprCapsule left, IBoolExprCapsule right) => context.CreateOrExpr(left.Item, right.Item) },
        { BinaryLogicalOperator.And, (SolverContext context, IBoolExprCapsule left, IBoolExprCapsule right) => context.CreateAndExpr(left.Item, right.Item) },
    };

    /// <summary>
    /// Gets the expression builder associated to a unary logical operator.
    /// </summary>
    public static Dictionary<UnaryLogicalOperator, Func<SolverContext, IBoolExprCapsule, IBoolExprCapsule>> UnaryLogical { get; } = new()
    {
        { UnaryLogicalOperator.Not, (SolverContext context, IBoolExprCapsule operand) => context.CreateNotExpr(operand.Item) },
    };

    /// <summary>
    /// Gets the expression builder associated to an equality operator.
    /// </summary>
    public static Dictionary<EqualityOperator, Func<SolverContext, IExprCapsule, IExprCapsule, IBoolExprCapsule>> Equality { get; } = new()
    {
        { EqualityOperator.Equal, (SolverContext context, IExprCapsule left, IExprCapsule right) => context.CreateEqualExpr(left.Item, right.Item) },
        { EqualityOperator.NotEqual, (SolverContext context, IExprCapsule left, IExprCapsule right) => context.CreateNotEqualExpr(left.Item, right.Item) },
    };

    /// <summary>
    /// Gets the expression builder associated to a comparison operator.
    /// </summary>
    public static Dictionary<ComparisonOperator, Func<SolverContext, IArithExprCapsule, IArithExprCapsule, IBoolExprCapsule>> Comparison { get; } = new()
    {
        { ComparisonOperator.GreaterThan, (SolverContext context, IArithExprCapsule left, IArithExprCapsule right) => context.CreateGreaterThanExpr(left.Item, right.Item) },
        { ComparisonOperator.GreaterThanOrEqual, (SolverContext context, IArithExprCapsule left, IArithExprCapsule right) => context.CreateGreaterThanEqualToExpr(left.Item, right.Item) },
        { ComparisonOperator.LessThan, (SolverContext context, IArithExprCapsule left, IArithExprCapsule right) => context.CreateLesserThanExpr(left.Item, right.Item) },
        { ComparisonOperator.LessThanOrEqual, (SolverContext context, IArithExprCapsule left, IArithExprCapsule right) => context.CreateLesserThanEqualToExpr(left.Item, right.Item) },
    };
}
