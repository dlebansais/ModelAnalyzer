namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Provides tables of supported operators.
/// </summary>
internal static class OperatorSyntaxKind
{
    /// <summary>
    /// Gets supported binary arithmetic operators.
    /// </summary>
    public static Dictionary<SyntaxKind, BinaryArithmeticOperator> BinaryArithmetic { get; } = new()
    {
        { SyntaxKind.PlusToken, BinaryArithmeticOperator.Add },
        { SyntaxKind.MinusToken, BinaryArithmeticOperator.Subtract },
        { SyntaxKind.AsteriskToken, BinaryArithmeticOperator.Multiply },
        { SyntaxKind.SlashToken, BinaryArithmeticOperator.Divide },
    };

    /// <summary>
    /// Gets supported unary arithmetic operators.
    /// </summary>
    public static Dictionary<SyntaxKind, UnaryArithmeticOperator> UnaryArithmetic { get; } = new()
    {
        { SyntaxKind.MinusToken, UnaryArithmeticOperator.Minus },
    };

    /// <summary>
    /// Gets supported binary logical operators.
    /// </summary>
    public static Dictionary<SyntaxKind, BinaryLogicalOperator> BinaryLogical { get; } = new()
    {
        { SyntaxKind.BarBarToken, BinaryLogicalOperator.Or },
        { SyntaxKind.AmpersandAmpersandToken, BinaryLogicalOperator.And },
    };

    /// <summary>
    /// Gets supported unary logical operators.
    /// </summary>
    public static Dictionary<SyntaxKind, UnaryLogicalOperator> UnaryLogical { get; } = new()
    {
        { SyntaxKind.ExclamationToken, UnaryLogicalOperator.Not },
    };

    /// <summary>
    /// Gets supported comparison operators.
    /// </summary>
    public static Dictionary<SyntaxKind, ComparisonOperator> Comparison { get; } = new()
    {
        { SyntaxKind.EqualsEqualsToken, ComparisonOperator.Equal },
        { SyntaxKind.ExclamationEqualsToken, ComparisonOperator.NotEqual },
        { SyntaxKind.GreaterThanToken, ComparisonOperator.GreaterThan },
        { SyntaxKind.GreaterThanEqualsToken, ComparisonOperator.GreaterThanOrEqual },
        { SyntaxKind.LessThanToken, ComparisonOperator.LessThan },
        { SyntaxKind.LessThanEqualsToken, ComparisonOperator.LessThanOrEqual },
    };
}
