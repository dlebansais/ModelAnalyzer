namespace ModelAnalyzer;

using System;
using Microsoft.Z3;

/// <summary>
/// Represents a unary arithmetic operator.
/// </summary>
internal record UnaryArithmeticOperator(string Text, Func<Context, ArithExpr, ArithExpr> Asserter);
