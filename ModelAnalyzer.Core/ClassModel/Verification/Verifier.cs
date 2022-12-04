namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using AnalysisLogger;
using Microsoft.Z3;

/// <summary>
/// Represents a code verifier.
/// </summary>
internal partial class Verifier : IDisposable
{
    public Verifier()
    {
        // Need model generation turned on.
        ctx = new Context(new Dictionary<string, string>() { { "model", "true" } });
        Zero = ctx.MkInt(0);
    }

    public void Verify()
    {
        Verify(0);
    }

    required public int MaxDepth { get; init; }
    required public string ClassName { get; init; }
    required public FieldTable FieldTable { get; init; }
    required public MethodTable MethodTable { get; init; }
    required public List<IInvariant> InvariantList { get; init; }
    public IAnalysisLogger Logger { get; init; } = new NullLogger();

    public bool IsInvariantViolated { get; private set; }

    private void Verify(int depth)
    {
        if (IsInvariantViolated)
            return;

        List<Method> CallSequence = new();
        CallMethods(depth, CallSequence);

        if (depth < MaxDepth)
            Verify(depth + 1);
    }

    private void CallMethods(int depth, List<Method> callSequence)
    {
        if (IsInvariantViolated)
            return;

        if (depth > 0)
        {
            foreach (KeyValuePair<MethodName, IMethod> Entry in MethodTable)
                if (Entry.Value is Method Method)
                {
                    List<Method> NewCallSequence = new();
                    NewCallSequence.AddRange(callSequence);
                    NewCallSequence.Add(Method);

                    CallMethods(depth - 1, NewCallSequence);
                }
        }
        else
            AddMethodCalls(callSequence);
    }

    private void AddMethodCalls(List<Method> callSequence)
    {
        Solver solver = ctx.MkSolver();
        AliasTable aliasTable = new();

        AddInitialState(solver, aliasTable);

        string CallSequenceString = string.Empty;
        foreach (Method Method in callSequence)
        {
            if (CallSequenceString.Length > 0)
                CallSequenceString += ", ";

            CallSequenceString += Method.Name;
        }

        if (CallSequenceString == string.Empty)
            Log($"Call sequence empty");
        else
            Log($"Call sequence: {CallSequenceString}");

        foreach (Method Method in callSequence)
            AddMethodCallState(solver, aliasTable, Method);

        AddClassInvariant(solver, aliasTable);

        if (solver.Check() == Status.SATISFIABLE)
        {
            IsInvariantViolated = true;
            Log($"Invariant violation for class {ClassName}");

            string ModelString = solver.Model.ToString();
            ModelString = ModelString.Replace("\n", "\r\n");
            Log(ModelString);
        }
        else
            Log($"Invariant preserved for class {ClassName}");
    }

    private void AddInitialState(Solver solver, AliasTable aliasTable)
    {
        Log($"Initial state for class {ClassName}");

        foreach (KeyValuePair<FieldName, IField> Entry in FieldTable)
        {
            string FieldName = Entry.Key.Name;
            aliasTable.AddName(FieldName);
            string FieldNameAlias = aliasTable.GetAlias(FieldName);

            IntExpr FieldExpr = ctx.MkIntConst(FieldNameAlias);
            BoolExpr InitExpr = ctx.MkEq(FieldExpr, Zero);

            Log($"Adding {InitExpr}");
            solver.Assert(InitExpr);
        }
    }

    private void AddClassInvariant(Solver solver, AliasTable aliasTable)
    {
        Log($"Invariant for class {ClassName}");

        List<BoolExpr> AssertionList = new();

        foreach (IInvariant Item in InvariantList)
            if (Item is Invariant Invariant)
            {
                BoolExpr InvariantExpression = BuildExpression<BoolExpr>(aliasTable, Invariant.BooleanExpression);
                AssertionList.Add(InvariantExpression);
            }

        BoolExpr AllInvariants = ctx.MkAnd(AssertionList);
        BoolExpr AllInvariantsOpposite = ctx.MkNot(AllInvariants);

        Log($"Adding invariant opposite {AllInvariantsOpposite}");
        solver.Assert(AllInvariantsOpposite);
    }

    private void AddMethodCallState(Solver solver, AliasTable aliasTable, Method method)
    {
        List<IntExpr> ExpressionList = new();

        foreach (KeyValuePair<ParameterName, IParameter> Entry in method.ParameterTable)
            if (Entry.Value is Parameter Parameter)
            {
                string ParameterName = Parameter.Name;
                aliasTable.AddName(ParameterName);
                string ParameterNameAlias = aliasTable.GetAlias(ParameterName);

                IntExpr ParameterExpr = ctx.MkIntConst(ParameterNameAlias);
                ExpressionList.Add(ParameterExpr);
            }

        foreach (IRequire Item in method.RequireList)
            if (Item is Require Require)
            {
                BoolExpr RequireExpr = BuildExpression<BoolExpr>(aliasTable, Require.BooleanExpression);
                solver.Assert(RequireExpr);
            }

        BoolExpr MainBranch = ctx.MkBool(true);

        AddStatementListExecution(solver, aliasTable, MainBranch, method.StatementList);
    }

    private void AddToSolver(Solver solver, BoolExpr branch, BoolExpr boolExpr)
    {
        solver.Assert(ctx.MkImplies(branch, boolExpr));
        Log($"Adding {branch} => {boolExpr}");
    }

    private void Log(string message)
    {
        Logger.Log(message);
    }

    public void Dispose()
    {
        ctx.Dispose();
    }

    private Context ctx;
    private IntExpr Zero;
}
