namespace DemoAnalyzer;

using System;
using Microsoft.Z3;

public record ComparisonOperator(string Text, Func<Context, ArithExpr, ArithExpr, BoolExpr> Asserter);
