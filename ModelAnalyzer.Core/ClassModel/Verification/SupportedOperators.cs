namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Z3;

/// <summary>
/// Provides tables of supported operators.
/// </summary>
internal static class SupportedOperators
{
    /// <summary>
    /// Gets supported comparison operators.
    /// </summary>
    public static Dictionary<SyntaxKind, ComparisonOperator> Comparison { get; } = new()
    {
        { SyntaxKind.EqualsEqualsToken, new ComparisonOperator("==", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkEq(left, right)) },
        { SyntaxKind.ExclamationEqualsToken, new ComparisonOperator("!=", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkNot(ctx.MkEq(left, right))) },
        { SyntaxKind.GreaterThanToken, new ComparisonOperator(">", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkGt(left, right)) },
        { SyntaxKind.GreaterThanEqualsToken, new ComparisonOperator(">=", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkGe(left, right)) },
        { SyntaxKind.LessThanToken, new ComparisonOperator("<", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkLt(left, right)) },
        { SyntaxKind.LessThanEqualsToken, new ComparisonOperator("<=", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkLe(left, right)) },
    };

    /// <summary>
    /// Gets supported arithmetic operators.
    /// </summary>
    public static Dictionary<SyntaxKind, ArithmeticOperator> Arithmetic { get; } = new()
    {
        { SyntaxKind.PlusToken, new ArithmeticOperator("+", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkAdd(left, right)) },
        { SyntaxKind.MinusToken, new ArithmeticOperator("-", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkSub(left, right)) },
        { SyntaxKind.AsteriskToken, new ArithmeticOperator("*", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkMul(left, right)) },
        { SyntaxKind.SlashToken, new ArithmeticOperator("/", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkDiv(left, right)) },
    };

    /// <summary>
    /// Gets supported logical operators.
    /// </summary>
    public static Dictionary<SyntaxKind, LogicalOperator> Logical { get; } = new()
    {
        { SyntaxKind.BarBarToken, new LogicalOperator("||", (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkOr(left, right)) },
        { SyntaxKind.AmpersandAmpersandToken, new LogicalOperator("&&", (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkAnd(left, right)) },
    };
}
