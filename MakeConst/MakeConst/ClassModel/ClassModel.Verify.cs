namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Z3;

public partial record ClassModel
{
    public void Verify()
    {
        // Need model generation turned on.
        using Context ctx = new Context(new Dictionary<string, string>() { { "model", "true" } });
        bool IsInvariantViolated = Verify(ctx);

        ClassModelManager.Instance.SetIsInvariantViolated(Name, IsInvariantViolated);
    }

    public void AddInitialState(Context ctx, Solver solver)
    {
        IntExpr zero = ctx.MkInt(0);

        foreach (KeyValuePair<FieldName, IField> Entry in FieldTable)
        {
            string FieldName = Entry.Key.Name;
            IntExpr FieldExpr = ctx.MkIntConst(FieldName);
            solver.Assert(ctx.MkEq(FieldExpr, zero));
            Logger.Log($"Adding {FieldName} == 0");
        }
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

    public void AddInvariants(Context ctx, Solver solver)
    {
        List<BoolExpr> AssertionList = new();

        foreach (IInvariant Item in InvariantList)
            if (Item is Invariant Invariant)
            {
                string FieldName = Invariant.FieldName;
                SyntaxKind OperatorKind = Invariant.OperatorKind;
                int ConstantValue = Invariant.ConstantValue;

                if (SupportedComparisonOperators.ContainsKey(OperatorKind))
                {
                    ComparisonOperator Operator = SupportedComparisonOperators[OperatorKind];
                    Logger.Log($"Adding {FieldName} {Operator.Text} {ConstantValue}");

                    IntExpr FieldExpr = ctx.MkIntConst(FieldName);
                    IntExpr ConstantExpr = ctx.MkInt(ConstantValue);
                    BoolExpr Assertion = Operator.Asserter(ctx, FieldExpr, ConstantExpr);
                    AssertionList.Add(Assertion);
                }
                else
                    throw new NotImplementedException($"Comparison operator {OperatorKind} not implemented");
            }

        solver.Assert(ctx.MkAnd(AssertionList));
    }

    public bool Verify(Context ctx)
    {
        Logger.Log("Solver starting");
        Solver solver = ctx.MkSolver();

        AddInitialState(ctx, solver);
        AddInvariants(ctx, solver);

        bool IsInvariantViolated = solver.Check() != Status.SATISFIABLE;
        Logger.Log($"Solver result: {IsInvariantViolated}");

        return IsInvariantViolated;
    }
}
