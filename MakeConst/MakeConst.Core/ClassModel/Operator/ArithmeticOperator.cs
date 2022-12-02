namespace DemoAnalyzer;

using System;
using Microsoft.Z3;

public record ArithmeticOperator(string Text, Func<Context, ArithExpr, ArithExpr, ArithExpr> Asserter);
