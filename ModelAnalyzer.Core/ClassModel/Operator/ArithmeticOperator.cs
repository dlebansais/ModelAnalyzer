namespace DemoAnalyzer;

using System;
using Microsoft.Z3;

/// <summary>
/// Represents an arithmetic operator.
/// </summary>
internal record ArithmeticOperator(string Text, Func<Context, ArithExpr, ArithExpr, ArithExpr> Asserter);
