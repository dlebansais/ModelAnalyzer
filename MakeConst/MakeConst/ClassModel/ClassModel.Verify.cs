namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using System.Reflection;
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

    public void AddInvariants(Context ctx, Solver solver)
    {
        List<BoolExpr> AssertionList = new();

        foreach (IInvariant Item in InvariantList)
            if (Item is Invariant Invariant)
            {
                string FieldName = Invariant.FieldName;
                string OperatorText = Invariant.OperatorText;
                int ConstantValue = Invariant.ConstantValue;

                IntExpr FieldExpr = ctx.MkIntConst(FieldName);
                IntExpr ConstantExpr = ctx.MkInt(ConstantValue);
                BoolExpr Assertion;

                Logger.Log($"Adding {FieldName} {OperatorText} {ConstantValue}");

                switch (OperatorText)
                {
                    case "==":
                        Assertion = ctx.MkEq(FieldExpr, ConstantExpr);
                        break;
                    case ">":
                        Assertion = ctx.MkGt(FieldExpr, ConstantExpr);
                        break;
                    case ">=":
                        Assertion = ctx.MkGe(FieldExpr, ConstantExpr);
                        break;
                    case "<":
                        Assertion = ctx.MkLt(FieldExpr, ConstantExpr);
                        break;
                    case "<=":
                        Assertion = ctx.MkLe(FieldExpr, ConstantExpr);
                        break;
                    default:
                        throw new NotImplementedException("Other operators are not implemented");
                }

                AssertionList.Add(Assertion);
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
