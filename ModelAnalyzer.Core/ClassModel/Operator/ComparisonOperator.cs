namespace DemoAnalyzer;

using System;
using Microsoft.Z3;

internal record ComparisonOperator(string Text, Func<Context, ArithExpr, ArithExpr, BoolExpr> Asserter);
