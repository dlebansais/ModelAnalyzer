namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Z3;

public partial record ClassModel
{
    const int MaxDepth = 1;

    public void Verify()
    {
        Verifier Verifier = new() { MaxDepth = MaxDepth, FieldTable = FieldTable, MethodTable = MethodTable, InvariantList = InvariantList };
        Verifier.Verify();

        ClassModelManager.Instance.SetIsInvariantViolated(Name, Verifier.IsInvariantViolated);

        Logger.Log("Pulsing event");
        PulseEvent.Set();
    }

    public void WaitForThreadCompleted()
    {
        bool IsCompleted = PulseEvent.WaitOne(TimeSpan.FromSeconds(2));

        Logger.Log($"Wait on event done, IsCompleted={IsCompleted}");
    }

    public static Dictionary<SyntaxKind, ComparisonOperator> SupportedComparisonOperators = new()
    {
        { SyntaxKind.EqualsEqualsToken, new ComparisonOperator("==", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkEq(left, right)) },
        { SyntaxKind.ExclamationEqualsToken, new ComparisonOperator("!=", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkNot(ctx.MkEq(left, right))) },
        { SyntaxKind.GreaterThanToken, new ComparisonOperator(">", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkGt(left, right)) },
        { SyntaxKind.GreaterThanEqualsToken, new ComparisonOperator(">=", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkGe(left, right)) },
        { SyntaxKind.LessThanToken, new ComparisonOperator("<", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkLt(left, right)) },
        { SyntaxKind.LessThanEqualsToken, new ComparisonOperator("<=", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkLe(left, right)) },
    };

    public static Dictionary<SyntaxKind, ArithmeticOperator> SupportedArithmeticOperators = new()
    {
        { SyntaxKind.PlusToken, new ArithmeticOperator("+", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkAdd(left, right)) },
        { SyntaxKind.MinusToken, new ArithmeticOperator("-", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkSub(left, right)) },
    };

    public static Dictionary<SyntaxKind, LogicalOperator> SupportedLogicalOperators = new()
    {
        { SyntaxKind.BarBarToken, new LogicalOperator("||", (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkOr(left, right)) },
        { SyntaxKind.AmpersandAmpersandToken, new LogicalOperator("&&", (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkAnd(left, right)) },
    };
}
