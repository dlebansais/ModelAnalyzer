namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Provides tables of supported operators.
/// </summary>
internal static class OperatorSyntaxKind
{
    /// <summary>
    /// Gets the arithmetic operators associated to a token.
    /// </summary>
    public static Dictionary<SyntaxKind, BinaryArithmeticOperator> BinaryArithmetic { get; } = new()
    {
        { SyntaxKind.PlusToken, BinaryArithmeticOperator.Add },
        { SyntaxKind.MinusToken, BinaryArithmeticOperator.Subtract },
        { SyntaxKind.AsteriskToken, BinaryArithmeticOperator.Multiply },
        { SyntaxKind.SlashToken, BinaryArithmeticOperator.Divide },
    };

    /// <summary>
    /// Gets the unary arithmetic operators associated to a token.
    /// </summary>
    public static Dictionary<SyntaxKind, UnaryArithmeticOperator> UnaryArithmetic { get; } = new()
    {
        { SyntaxKind.MinusToken, UnaryArithmeticOperator.Minus },
    };

    /// <summary>
    /// Gets the binary logical operators associated to a token.
    /// </summary>
    public static Dictionary<SyntaxKind, BinaryLogicalOperator> BinaryLogical { get; } = new()
    {
        { SyntaxKind.BarBarToken, BinaryLogicalOperator.Or },
        { SyntaxKind.AmpersandAmpersandToken, BinaryLogicalOperator.And },
    };

    /// <summary>
    /// Gets the unary logical operators associated to a token.
    /// </summary>
    public static Dictionary<SyntaxKind, UnaryLogicalOperator> UnaryLogical { get; } = new()
    {
        { SyntaxKind.ExclamationToken, UnaryLogicalOperator.Not },
    };

    /// <summary>
    /// Gets the equality operators associated to a token.
    /// </summary>
    public static Dictionary<SyntaxKind, EqualityOperator> Equality { get; } = new()
    {
        { SyntaxKind.EqualsEqualsToken, EqualityOperator.Equal },
        { SyntaxKind.ExclamationEqualsToken, EqualityOperator.NotEqual },
    };

    /// <summary>
    /// Gets the comparison operators associated to a token.
    /// </summary>
    public static Dictionary<SyntaxKind, ComparisonOperator> Comparison { get; } = new()
    {
        { SyntaxKind.GreaterThanToken, ComparisonOperator.GreaterThan },
        { SyntaxKind.GreaterThanEqualsToken, ComparisonOperator.GreaterThanOrEqual },
        { SyntaxKind.LessThanToken, ComparisonOperator.LessThan },
        { SyntaxKind.LessThanEqualsToken, ComparisonOperator.LessThanOrEqual },
    };
}
