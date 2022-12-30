namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AnalysisLogger;
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
    required public ReadOnlyFieldTable FieldTable { get; init; }

    /// <summary>
    /// Gets the method table.
    /// </summary>
    required public ReadOnlyMethodTable MethodTable { get; init; }

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

        CallSequence CallSequence = new();
        CallMethods(depth, CallSequence);

        if (depth < MaxDepth)
            Verify(depth + 1);
    }

    private void CallMethods(int depth, CallSequence callSequence)
    {
        if (depth > 0)
        {
            foreach (KeyValuePair<MethodName, Method> Entry in MethodTable)
            {
                Method Method = Entry.Value;

                if (Method.AccessModifier == AccessModifier.Public)
                {
                    CallSequence NewCallSequence = callSequence.WithAddedCall(Method);

                    CallMethods(depth - 1, NewCallSequence);
                }
            }
        }
        else
            AddMethodCalls(callSequence);
    }

    private void AddMethodCalls(CallSequence callSequence)
    {
        using Solver solver = Context.MkSolver();

        AliasTable aliasTable = new();

        AddInitialState(solver, aliasTable);

        if (callSequence.IsEmpty)
            Log($"Call sequence empty");
        else
            Log($"Call sequence: {callSequence}");

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
            Field Field = Entry.Value;
            Variable FieldVariable = new(Field.Name, Field.Type);

            aliasTable.AddVariable(FieldVariable);

            VariableAlias FieldNameAlias = aliasTable.GetAlias(FieldVariable);
            Expr FieldExpr = CreateVariableExpr(FieldNameAlias.ToString(), Field.Type);
            Expr InitializerExpr = CreateFieldInitializer(Field);
            BoolExpr InitExpr = Context.MkEq(FieldExpr, InitializerExpr);

            Log($"Adding {InitExpr}");
            solver.Assert(InitExpr);
        }
    }

    private Expr CreateVariableExpr(string aliasString, ExpressionType variableType)
    {
        Dictionary<ExpressionType, Func<string, Expr>> SwitchTable = new()
        {
            { ExpressionType.Boolean, Context.MkBoolConst },
            { ExpressionType.Integer, Context.MkIntConst },
            { ExpressionType.FloatingPoint, Context.MkRealConst },
        };

        Debug.Assert(SwitchTable.ContainsKey(variableType));
        Expr Result = SwitchTable[variableType](aliasString);

        return Result;
    }

    private Expr CreateFieldInitializer(Field field)
    {
        ExpressionType FieldType = field.Type;
        Dictionary<ExpressionType, Func<Field, Expr>> SwitchTable = new()
        {
            { ExpressionType.Boolean, CreateBooleanFieldInitializer },
            { ExpressionType.Integer, CreateIntegerFieldInitializer },
            { ExpressionType.FloatingPoint, CreateFloatingPointFieldInitializer },
        };

        Debug.Assert(SwitchTable.ContainsKey(FieldType));
        Expr Result = SwitchTable[FieldType](field);

        return Result;
    }

    private Expr CreateBooleanFieldInitializer(Field field)
    {
        if (field.Initializer is LiteralBooleanValueExpression LiteralBoolean)
            return LiteralBoolean.Value == true ? True : False;
        else
            return False;
    }

    private Expr CreateIntegerFieldInitializer(Field field)
    {
        if (field.Initializer is LiteralIntegerValueExpression LiteralInteger)
            return LiteralInteger.Value == 0 ? Zero : CreateIntegerExpr(LiteralInteger.Value);
        else
            return Zero;
    }

    private Expr CreateFloatingPointFieldInitializer(Field field)
    {
        if (field.Initializer is LiteralFloatingPointValueExpression LiteralFloatingPoint)
            return LiteralFloatingPoint.Value == 0 ? Zero : CreateFloatingPointExpr(LiteralFloatingPoint.Value);
        else
            return Zero;
    }

    private Expr CreateVariableInitializer(ExpressionType variableType)
    {
        Dictionary<ExpressionType, Expr> SwitchTable = new()
        {
            { ExpressionType.Boolean, False },
            { ExpressionType.Integer, Zero },
            { ExpressionType.FloatingPoint, Zero },
        };

        Debug.Assert(SwitchTable.ContainsKey(variableType));
        Expr Result = SwitchTable[variableType];

        return Result;
    }

    private bool AddClassInvariant(Solver solver, AliasTable aliasTable)
    {
        bool Result = true;

        Log($"Invariant for class {ClassName}");

        for (int i = 0; i < InvariantList.Count && Result == true; i++)
        {
            Invariant Invariant = InvariantList[i];
            BoolExpr InvariantExpression = BuildExpression<BoolExpr>(aliasTable, hostMethod: null, resultField: null, Invariant.BooleanExpression);
            BoolExpr InvariantOpposite = Context.MkNot(InvariantExpression);

            solver.Push();

            Log($"Adding invariant opposite {InvariantOpposite}");
            solver.Assert(InvariantOpposite);

            if (solver.Check() == Status.SATISFIABLE)
            {
                Log($"Invariant violation for class {ClassName}");
                VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.InvariantError, ClassName = ClassName, MethodName = string.Empty, ErrorIndex = i };

                string ModelString = TextBuilder.Normalized(solver.Model.ToString());
                Log(ModelString);

                Result = false;
            }

            solver.Pop();
        }

        return Result;
    }

    private bool AddMethodCallState(Solver solver, AliasTable aliasTable, Method method)
    {
        AddMethodParameterStates(solver, aliasTable, method);
        if (!AddMethodRequires(solver, aliasTable, method, checkOpposite: false))
            return false;

        BoolExpr MainBranch = Context.MkBool(true);
        Field? ResultField = null;

        if (!AddStatementListExecution(solver, aliasTable, method, ref ResultField, MainBranch, method.StatementList))
            return false;

        if (!AddMethodEnsures(solver, aliasTable, method, ResultField, keepNormal: false))
            return false;

        return true;
    }

    private void AddMethodParameterStates(Solver solver, AliasTable aliasTable, Method method)
    {
        foreach (KeyValuePair<ParameterName, Parameter> Entry in method.ParameterTable)
        {
            Parameter Parameter = Entry.Value;
            ParameterName ParameterLocalName = CreateParameterLocalName(method, Parameter);
            Variable ParameterVariable = new(ParameterLocalName, Parameter.Type);

            aliasTable.AddOrIncrement(ParameterVariable);

            VariableAlias ParameterLocalAlias = aliasTable.GetAlias(ParameterVariable);

            CreateVariableExpr(ParameterLocalAlias.ToString(), Parameter.Type);
        }
    }

    private static ParameterName CreateParameterLocalName(Method method, Parameter parameter)
    {
        string ParameterLocalText = $"{method.Name.Text}${parameter.Name.Text}";
        return new ParameterName() { Text = ParameterLocalText };
    }

    // checkOpposite: false for a call outside the call (the call is assumed to fulfill the contract)
    //                true for a call from within the class (the call must fulfill the contract)
    private bool AddMethodRequires(Solver solver, AliasTable aliasTable, Method method, bool checkOpposite)
    {
        for (int i = 0; i < method.RequireList.Count; i++)
        {
            Require Require = method.RequireList[i];
            BoolExpr AssertionExpr = BuildExpression<BoolExpr>(aliasTable, method, resultField: null, Require.BooleanExpression);
            string AssertionType = "require";
            VerificationErrorType ErrorType = VerificationErrorType.RequireError;

            if (checkOpposite && !AddMethodAssertionOpposite(solver, method, AssertionExpr, i, AssertionType, ErrorType))
                return false;

            if (!AddMethodAssertionNormal(solver, method, AssertionExpr, i, AssertionType, ErrorType, keepNormal: true))
                return false;
        }

        return true;
    }

    private bool AddMethodEnsures(Solver solver, AliasTable aliasTable, Method method, Field? resultField, bool keepNormal)
    {
        for (int i = 0; i < method.EnsureList.Count; i++)
        {
            Ensure Ensure = method.EnsureList[i];
            BoolExpr AssertionExpr = BuildExpression<BoolExpr>(aliasTable, method, resultField, Ensure.BooleanExpression);
            string AssertionType = "ensure";
            VerificationErrorType ErrorType = VerificationErrorType.EnsureError;

            if (!AddMethodAssertionNormal(solver, method, AssertionExpr, i, AssertionType, ErrorType, keepNormal))
                return false;

            if (!AddMethodAssertionOpposite(solver, method, AssertionExpr, i, AssertionType, ErrorType))
                return false;
        }

        return true;
    }

    // keepNormal: true if we keep the contract (for all require, and for ensure after a call from within the class, since we must fulfill the contract. From outside the class, we no longer care)
    private bool AddMethodAssertionNormal(Solver solver, Method method, BoolExpr assertionExpr, int index, string assertionType, VerificationErrorType errorType, bool keepNormal)
    {
        bool Result = true;

        if (!keepNormal)
            solver.Push();

        Log($"Adding {assertionType} {assertionExpr}");
        solver.Assert(assertionExpr);

        if (solver.Check() != Status.SATISFIABLE)
        {
            Log($"Inconsistent {assertionType} state for class {ClassName}");
            VerificationResult = VerificationResult.Default with { ErrorType = errorType, ClassName = ClassName, MethodName = method.Name.Text, ErrorIndex = index };

            Result = false;
        }

        if (!keepNormal)
            solver.Pop();

        return Result;
    }

    private bool AddMethodAssertionOpposite(Solver solver, Method method, BoolExpr assertionExpr, int index, string assertionType, VerificationErrorType errorType)
    {
        bool Result = true;

        BoolExpr AssertionOppositeExpr = Context.MkNot(assertionExpr);

        solver.Push();

        Log($"Adding {assertionType} opposite {AssertionOppositeExpr}");
        solver.Assert(AssertionOppositeExpr);

        if (solver.Check() == Status.SATISFIABLE)
        {
            Log($"Violation of {assertionType} for class {ClassName}");
            VerificationResult = VerificationResult.Default with { ErrorType = errorType, ClassName = ClassName, MethodName = method.Name.Text, ErrorIndex = index };

            string ModelString = TextBuilder.Normalized(solver.Model.ToString());
            Log(ModelString);

            Result = false;
        }

        solver.Pop();

        return Result;
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

    private Context Context;
    private IntExpr Zero;
    private BoolExpr False;
    private BoolExpr True;
}
