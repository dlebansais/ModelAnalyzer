namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using AnalysisLogger;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.Extensions.Logging;
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
        False = Context.MkBool(false);
        True = Context.MkBool(true);
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
    public VerificationResult VerificationResult { get; private set; } = VerificationResult.Default;

    private void Verify(int depth)
    {
        // Stop analysing recursively if a violation has already been detected.
        if (VerificationResult != VerificationResult.Default && VerificationResult.IsError)
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
                Method Method = Entry.Value;
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
            if (!AddMethodCallState(solver, aliasTable, Method))
                return;

        if (!AddClassInvariant(solver, aliasTable))
            return;

        VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.Success, ClassName = ClassName };
        Log($"Invariant preserved for class {ClassName}");
    }

    private void AddInitialState(Solver solver, AliasTable aliasTable)
    {
        Log($"Initial state for class {ClassName}");

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
        {
            string FieldName = Entry.Key.Name;
            Field Field = Entry.Value;

            aliasTable.AddName(FieldName);
            string FieldNameAlias = aliasTable.GetAlias(FieldName);

            IntExpr FieldExpr = Context.MkIntConst(FieldNameAlias);
            Expr Initializer = GetFieldInitializer(Field);
            BoolExpr InitExpr = Context.MkEq(FieldExpr, Initializer);

            Log($"Adding {InitExpr}");
            solver.Assert(InitExpr);
        }
    }

    private Expr GetFieldInitializer(Field field)
    {
        Expr Initializer = null!;
        bool IsHandled = false;

        switch (field.VariableType)
        {
            case ExpressionType.Boolean:
                if (field.Initializer is LiteralBoolValueExpression LiteralBoolean)
                    Initializer = LiteralBoolean.Value == true ? True : False;
                else
                    Initializer = False;
                IsHandled = true;
                break;

            case ExpressionType.Integer:
                if (field.Initializer is LiteralIntValueExpression LiteralInteger)
                    Initializer = LiteralInteger.Value == 0 ? Zero : CreateIntegerExpr(LiteralInteger.Value);
                else
                    Initializer = Zero;
                IsHandled = true;
                break;

            case ExpressionType.FloatingPoint:
                if (field.Initializer is LiteralFloatingPointValueExpression LiteralFloatingPoint)
                    Initializer = LiteralFloatingPoint.Value == 0 ? Zero : CreateFloatingPointExpr(LiteralFloatingPoint.Value);
                else
                    Initializer = Zero;
                IsHandled = true;
                break;
        }

        Debug.Assert(IsHandled);

        return Initializer;
    }

    private bool AddClassInvariant(Solver solver, AliasTable aliasTable)
    {
        Log($"Invariant for class {ClassName}");

        for (int i = 0; i < InvariantList.Count; i++)
        {
            Invariant Invariant = InvariantList[i];
            BoolExpr InvariantExpression = BuildExpression<BoolExpr>(aliasTable, Invariant.BooleanExpression);
            BoolExpr InvariantOpposite = Context.MkNot(InvariantExpression);

            Log($"Adding invariant opposite {InvariantOpposite}");
            solver.Assert(InvariantOpposite);

            if (solver.Check() == Status.SATISFIABLE)
            {
                Log($"Invariant violation for class {ClassName}");
                VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.InvariantError, ClassName = ClassName, MethodName = string.Empty, ErrorIndex = i };

                string ModelString = TextBuilder.Normalized(solver.Model.ToString());
                Log(ModelString);

                return false;
            }
        }

        return true;
    }

    private bool AddMethodCallState(Solver solver, AliasTable aliasTable, Method method)
    {
        AddMethodParameterStates(solver, aliasTable, method);
        if (!AddMethodRequires(solver, aliasTable, method))
            return false;

        BoolExpr MainBranch = Context.MkBool(true);

        AddStatementListExecution(solver, aliasTable, MainBranch, method.StatementList);
        if (!AddMethodEnsures(solver, aliasTable, method))
            return false;

        return true;
    }

    private void AddMethodParameterStates(Solver solver, AliasTable aliasTable, Method method)
    {
        foreach (KeyValuePair<ParameterName, Parameter> Entry in method.ParameterTable)
        {
            Parameter Parameter = Entry.Value;
            string ParameterName = Parameter.Name;
            aliasTable.AddOrIncrementName(ParameterName);
            string ParameterNameAlias = aliasTable.GetAlias(ParameterName);

            Context.MkIntConst(ParameterNameAlias);
        }
    }

    private bool AddMethodRequires(Solver solver, AliasTable aliasTable, Method method)
    {
        for (int i = 0; i < method.RequireList.Count; i++)
        {
            Require Require = method.RequireList[i];
            BoolExpr RequireExpr = BuildExpression<BoolExpr>(aliasTable, Require.BooleanExpression);

            Log($"Adding {RequireExpr}");
            solver.Assert(RequireExpr);

            if (solver.Check() != Status.SATISFIABLE)
            {
                Log($"Inconsistent require state for class {ClassName}");
                VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.RequireError, ClassName = ClassName, MethodName = method.Name, ErrorIndex = i };
                return false;
            }
        }

        return true;
    }

    private bool AddMethodEnsures(Solver solver, AliasTable aliasTable, Method method)
    {
        for (int i = 0; i < method.EnsureList.Count; i++)
        {
            Ensure Ensure = method.EnsureList[i];
            BoolExpr EnsureExpr = BuildExpression<BoolExpr>(aliasTable, Ensure.BooleanExpression);

            Log($"Adding {EnsureExpr}");
            solver.Assert(EnsureExpr);

            if (solver.Check() != Status.SATISFIABLE)
            {
                Log($"Inconsistent ensure state for class {ClassName}");
                VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.EnsureError, ClassName = ClassName, MethodName = method.Name, ErrorIndex = i };
                return false;
            }
        }

        return true;
    }

    private void AddToSolver(Solver solver, BoolExpr branch, BoolExpr boolExpr)
    {
        solver.Assert(Context.MkImplies(branch, boolExpr));
        Log($"Adding {branch} => {boolExpr}");
    }

    private BoolExpr CreateBooleanExpr(bool value)
    {
        return Context.MkBool(value);
    }

    private IntExpr CreateIntegerExpr(int value)
    {
        return Context.MkInt(value);
    }

    private ArithExpr CreateFloatingPointExpr(double value)
    {
        return (ArithExpr)Context.MkNumeral(value.ToString(CultureInfo.InvariantCulture), Context.MkRealSort());
    }

    private void Log(string message)
    {
        Logger.Log(message);
    }

    private void LogError(string message)
    {
        Logger.Log(LogLevel.Error, message);
    }

    private Context Context;
    private IntExpr Zero;
    private BoolExpr False;
    private BoolExpr True;
}
