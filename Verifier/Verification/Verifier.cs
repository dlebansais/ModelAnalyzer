namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using AnalysisLogger;
using Microsoft.Z3;

/// <summary>
/// Represents a code verifier.
/// </summary>
internal partial class Verifier : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Verifier"/> class.
    /// </summary>
    public Verifier()
    {
        // Need model generation turned on.
        Context = new Context(new Dictionary<string, string>() { { "model", "true" } });
        Zero = Context.MkInt(0);
    }

    /// <summary>
    /// Verifies the code.
    /// </summary>
    public void Verify()
    {
        Verify(depth: 0);
    }

    /// <summary>
    /// Gets the max depth.
    /// </summary>
    required public int MaxDepth { get; init; }

    /// <summary>
    /// Gets the class name.
    /// </summary>
    required public string ClassName { get; init; }

    /// <summary>
    /// Gets the field table.
    /// </summary>
    required public FieldTable FieldTable { get; init; }

    /// <summary>
    /// Gets the method table.
    /// </summary>
    required public MethodTable MethodTable { get; init; }

    /// <summary>
    /// Gets the invariant list.
    /// </summary>
    required public List<Invariant> InvariantList { get; init; }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    public IAnalysisLogger Logger { get; init; } = new NullLogger();

    /// <summary>
    /// Gets a value indicating whether the invariant is violated.
    /// </summary>
    public bool IsInvariantViolated { get; private set; }

    private void Verify(int depth)
    {
        // Stop analysing recursively if a violation has already been detected.
        if (IsInvariantViolated)
            return;

        List<Method> CallSequence = new();
        CallMethods(depth, CallSequence);

        if (depth < MaxDepth)
            Verify(depth + 1);
    }

    private void CallMethods(int depth, List<Method> callSequence)
    {
        if (depth > 0)
        {
            foreach (KeyValuePair<MethodName, Method> Entry in MethodTable)
            {
                Method Method = (Method)Entry.Value; // This line can only be executed if there are no unsupported methods.
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
        using Solver solver = Context.MkSolver();

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

            string ModelString = TextBuilder.Normalized(solver.Model.ToString());
            Log(ModelString);
        }
        else
            Log($"Invariant preserved for class {ClassName}");
    }

    private void AddInitialState(Solver solver, AliasTable aliasTable)
    {
        Log($"Initial state for class {ClassName}");

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
        {
            string FieldName = Entry.Key.Name;
            aliasTable.AddName(FieldName);
            string FieldNameAlias = aliasTable.GetAlias(FieldName);

            IntExpr FieldExpr = Context.MkIntConst(FieldNameAlias);
            BoolExpr InitExpr = Context.MkEq(FieldExpr, Zero);

            Log($"Adding {InitExpr}");
            solver.Assert(InitExpr);
        }
    }

    private void AddClassInvariant(Solver solver, AliasTable aliasTable)
    {
        Log($"Invariant for class {ClassName}");

        List<BoolExpr> AssertionList = new();

        foreach (IInvariant Item in InvariantList)
        {
            Invariant Invariant = (Invariant)Item; // This line can only be executed if there are no unsupported invariants.
            BoolExpr InvariantExpression = BuildExpression<BoolExpr>(aliasTable, Invariant.BooleanExpression);
            AssertionList.Add(InvariantExpression);
        }

        BoolExpr AllInvariants = Context.MkAnd(AssertionList);
        BoolExpr AllInvariantsOpposite = Context.MkNot(AllInvariants);

        Log($"Adding invariant opposite {AllInvariantsOpposite}");
        solver.Assert(AllInvariantsOpposite);
    }

    private void AddMethodCallState(Solver solver, AliasTable aliasTable, Method method)
    {
        AddMethodParameterStates(solver, aliasTable, method);
        AddMethodRequires(solver, aliasTable, method);

        BoolExpr MainBranch = Context.MkBool(true);

        AddStatementListExecution(solver, aliasTable, MainBranch, method.StatementList);
        AddMethodEnsures(solver, aliasTable, method);
    }

    private void AddMethodParameterStates(Solver solver, AliasTable aliasTable, Method method)
    {
        foreach (KeyValuePair<ParameterName, Parameter> Entry in method.ParameterTable)
        {
            Parameter Parameter = (Parameter)Entry.Value; // This line can only be executed if there are no unsupported parameters.
            string ParameterName = Parameter.Name;
            aliasTable.AddOrIncrementName(ParameterName);
            string ParameterNameAlias = aliasTable.GetAlias(ParameterName);

            Context.MkIntConst(ParameterNameAlias);
        }
    }

    private void AddMethodRequires(Solver solver, AliasTable aliasTable, Method method)
    {
        foreach (IRequire Item in method.RequireList)
        {
            Require Require = (Require)Item; // This line can only be executed if there are no unsupported require.
            BoolExpr RequireExpr = BuildExpression<BoolExpr>(aliasTable, Require.BooleanExpression);
            solver.Assert(RequireExpr);
        }
    }

    private void AddMethodEnsures(Solver solver, AliasTable aliasTable, Method method)
    {
        foreach (IEnsure Item in method.EnsureList)
        {
            Ensure Ensure = (Ensure)Item; // This line can only be executed if there are no unsupported ensure.
            BoolExpr EnsureExpr = BuildExpression<BoolExpr>(aliasTable, Ensure.BooleanExpression);
            solver.Assert(EnsureExpr);
        }
    }

    private void AddToSolver(Solver solver, BoolExpr branch, BoolExpr boolExpr)
    {
        solver.Assert(Context.MkImplies(branch, boolExpr));
        Log($"Adding {branch} => {boolExpr}");
    }

    private void Log(string message)
    {
        Logger.Log(message);
    }

    private Context Context;
    private IntExpr Zero;
}
