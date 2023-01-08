namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        False = Context.MkBool(false);
        True = Context.MkBool(true);
    }

    /// <summary>
    /// Verifies the code.
    /// </summary>
    public void Verify()
    {
        VerificationWatch.Restart();

        Verify(depth: 0);
    }

    /// <summary>
    /// Gets the max depth.
    /// </summary>
    required public int MaxDepth { get; init; }

    /// <summary>
    /// Gets the max duration.
    /// </summary>
    required public TimeSpan MaxDuration { get; init; }

    /// <summary>
    /// Gets the class models.
    /// </summary>
    required public Dictionary<string, ClassModel> ClassModelTable { get; init; }

    /// <summary>
    /// Gets the class name.
    /// </summary>
    required public string ClassName { get; init; }

    /// <summary>
    /// Gets the property table.
    /// </summary>
    required public ReadOnlyPropertyTable PropertyTable { get; init; }

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
    required public IReadOnlyList<Invariant> InvariantList { get; init; }

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
        // Stop analysing recursively if interrupted.
        if (IsAnalysisInterrupted())
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

                    // Stop analysing more sequences if interrupted.
                    if (IsAnalysisInterrupted())
                        return;
                }
            }
        }
        else
            AnalyzeCallSequence(callSequence);
    }

    private bool IsAnalysisInterrupted()
    {
        // Stop analysing if a violation has already been detected.
        if (VerificationResult != VerificationResult.Default && VerificationResult.IsError)
            return true;

        // Stop analysing if this is taking too much time.
        if (VerificationWatch.Elapsed >= MaxDuration)
        {
            VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.Timeout };
            Log($"Timeout");

            return true;
        }

        return false;
    }

    private void AnalyzeCallSequence(CallSequence callSequence)
    {
        using Solver Solver = Context.MkSolver();
        VerificationContext VerificationContext = new() { Solver = Solver, ClassModelTable = ClassModelTable, PropertyTable = PropertyTable, FieldTable = FieldTable, MethodTable = MethodTable };

        AddInitialState(VerificationContext);

        if (callSequence.IsEmpty)
            Log($"Call sequence empty");
        else
            Log($"Call sequence: {callSequence}");

        foreach (Method Method in callSequence)
            if (!AddMethodCallStateWithInit(VerificationContext, Method))
                return;

        if (!AddClassInvariant(VerificationContext))
            return;

        VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.Success, ClassName = ClassName };
        Log($"Invariant preserved for class {ClassName}");
    }

    private void AddInitialState(VerificationContext verificationContext)
    {
        Log($"Initial state for class {ClassName}");

        AliasTable AliasTable = verificationContext.AliasTable;

        foreach (KeyValuePair<PropertyName, Property> Entry in verificationContext.PropertyTable)
        {
            Property Property = Entry.Value;
            Variable PropertyVariable = new(Property.Name, Property.Type);

            AliasTable.AddVariable(PropertyVariable);

            VariableAlias PropertyNameAlias = AliasTable.GetAlias(PropertyVariable);
            Expr PropertyExpr = CreateVariableExpr(verificationContext, PropertyNameAlias.ToString(), Property.Type);
            Expr InitializerExpr = CreateInitializerExpr(Property);
            BoolExpr InitExpr = Context.MkEq(PropertyExpr, InitializerExpr);

            Log($"Adding {InitExpr}");
            verificationContext.Solver.Assert(InitExpr);
        }

        foreach (KeyValuePair<FieldName, Field> Entry in verificationContext.FieldTable)
        {
            Field Field = Entry.Value;
            Variable FieldVariable = new(Field.Name, Field.Type);

            AliasTable.AddVariable(FieldVariable);

            VariableAlias FieldNameAlias = AliasTable.GetAlias(FieldVariable);
            Expr FieldExpr = CreateVariableExpr(verificationContext, FieldNameAlias.ToString(), Field.Type);
            Expr InitializerExpr = CreateInitializerExpr(Field);
            BoolExpr InitExpr = Context.MkEq(FieldExpr, InitializerExpr);

            Log($"Adding {InitExpr}");
            verificationContext.Solver.Assert(InitExpr);
        }
    }

    private Local FindOrCreateResultLocal(VerificationContext verificationContext, ExpressionType returnType)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;
        AliasTable AliasTable = verificationContext.AliasTable;

        foreach (KeyValuePair<LocalName, Local> Entry in HostMethod.LocalTable)
            if (Entry.Key.Text == Ensure.ResultKeyword)
                return Entry.Value;

        LocalName ResultLocalName = new LocalName() { Text = Ensure.ResultKeyword };
        Local ResultLocal = new Local() { Name = ResultLocalName, Type = returnType, Initializer = null };
        LocalName ResultLocalBlockName = CreateLocalBlockName(HostMethod, ResultLocal);
        Variable ResultLocalVariable = new(ResultLocalBlockName, returnType);

        AliasTable.AddOrIncrement(ResultLocalVariable);

        return ResultLocal;
    }

    private Expr CreateVariableExpr(VerificationContext verificationContext, string aliasString, ExpressionType variableType)
    {
        Debug.Assert(variableType != ExpressionType.Other);

        Expr Result;

        Dictionary<ExpressionType, Func<string, Expr>> SwitchTable = new()
        {
            { ExpressionType.Boolean, Context.MkBoolConst },
            { ExpressionType.Integer, Context.MkIntConst },
            { ExpressionType.FloatingPoint, Context.MkRealConst },
        };

        if (SwitchTable.ContainsKey(variableType))
            Result = SwitchTable[variableType](aliasString);
        else
        {
            string ClassName = variableType.Name;

            Debug.Assert(verificationContext.ClassModelTable.ContainsKey(ClassName));

            ClassModel TypeClassModel = verificationContext.ClassModelTable[ClassName];
            Result = Context.MkIntConst(aliasString);

            foreach (KeyValuePair<PropertyName, Property> Entry in TypeClassModel.PropertyTable)
            {
                // TODO: properties
            }
        }

        return Result;
    }

    private Expr CreateInitializerExpr(IVariableWithInitializer variable)
    {
        ExpressionType VariableType = variable.Type;
        Dictionary<ExpressionType, Func<IVariableWithInitializer, Expr>> SwitchTable = new()
        {
            { ExpressionType.Boolean, CreateBooleanInitializer },
            { ExpressionType.Integer, CreateIntegerInitializer },
            { ExpressionType.FloatingPoint, CreateFloatingPointInitializer },
        };

        Expr Result;

        if (SwitchTable.ContainsKey(VariableType))
            Result = SwitchTable[VariableType](variable);
        else if (variable.Initializer is LiteralNullExpression LiteralNull)
            return Zero; // TODO
        else
            Result = CreateObjectInitializer(VariableType);

        return Result;
    }

    private Expr CreateBooleanInitializer(IVariableWithInitializer variable)
    {
        if (variable.Initializer is LiteralBooleanValueExpression LiteralBoolean)
            return LiteralBoolean.Value == true ? True : False;
        else
            return False;
    }

    private Expr CreateIntegerInitializer(IVariableWithInitializer variable)
    {
        if (variable.Initializer is LiteralIntegerValueExpression LiteralInteger)
            return LiteralInteger.Value == 0 ? Zero : CreateIntegerExpr(LiteralInteger.Value);
        else
            return Zero;
    }

    private Expr CreateFloatingPointInitializer(IVariableWithInitializer variable)
    {
        if (variable.Initializer is LiteralFloatingPointValueExpression LiteralFloatingPoint)
            return LiteralFloatingPoint.Value == 0 ? Zero : CreateFloatingPointExpr(LiteralFloatingPoint.Value);
        else
            return Zero;
    }

    private Expr CreateObjectInitializer(ExpressionType expressionType)
    {
        // TODO
        return Zero;
    }

    private Expr GetDefaultExpr(ExpressionType variableType)
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

    private bool AddClassInvariant(VerificationContext verificationContext)
    {
        bool Result = true;

        Log($"Invariant for class {ClassName}");

        for (int i = 0; i < InvariantList.Count && Result == true; i++)
        {
            Invariant Invariant = InvariantList[i];

            if (BuildExpression(verificationContext, Invariant.BooleanExpression, out BoolExpr InvariantExpression))
            {
                BoolExpr InvariantOpposite = Context.MkNot(InvariantExpression);

                verificationContext.Solver.Push();

                Log($"Adding invariant opposite {InvariantOpposite}");
                verificationContext.Solver.Assert(InvariantOpposite);

                if (verificationContext.Solver.Check() == Status.SATISFIABLE)
                {
                    Log($"Invariant violation for class {ClassName}");
                    VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.InvariantError, ClassName = ClassName, MethodName = string.Empty, ErrorIndex = i };

                    string ModelString = TextBuilder.Normalized(verificationContext.Solver.Model.ToString());
                    Log(ModelString);

                    Result = false;
                }

                verificationContext.Solver.Pop();
            }
            else
                Result = false;
        }

        return Result;
    }

    private bool AddMethodCallStateWithInit(VerificationContext verificationContext, Method hostMethod)
    {
        VerificationContext LocalVerificationContext = verificationContext with { HostMethod = hostMethod };
        Local? ResultLocal = hostMethod.ReturnType != ExpressionType.Void ? FindOrCreateResultLocal(LocalVerificationContext, hostMethod.ReturnType) : null;
        VerificationContext StatementVerificationContext = LocalVerificationContext with { ResultLocal = ResultLocal };

        return AddMethodCallState(StatementVerificationContext);
    }

    private bool AddMethodCallState(VerificationContext verificationContext)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;

        AddMethodParameterStates(verificationContext);
        if (!AddMethodRequires(verificationContext, checkOpposite: false))
            return false;

        AddMethodLocalStates(verificationContext);

        if (!AddStatementListExecution(verificationContext, HostMethod.StatementList))
            return false;

        if (!AddMethodEnsures(verificationContext, keepNormal: false))
            return false;

        return true;
    }

    private void AddMethodParameterStates(VerificationContext verificationContext)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;
        AliasTable AliasTable = verificationContext.AliasTable;

        foreach (KeyValuePair<ParameterName, Parameter> Entry in HostMethod.ParameterTable)
        {
            Parameter Parameter = Entry.Value;
            ParameterName ParameterBlockName = CreateParameterBlockName(HostMethod, Parameter);
            Variable ParameterVariable = new(ParameterBlockName, Parameter.Type);

            AliasTable.AddOrIncrement(ParameterVariable);

            VariableAlias ParameterBlockAlias = AliasTable.GetAlias(ParameterVariable);

            CreateVariableExpr(verificationContext, ParameterBlockAlias.ToString(), Parameter.Type);
        }
    }

    private static ParameterName CreateParameterBlockName(Method method, Parameter parameter)
    {
        string ParameterBlockText = $"{method.Name.Text}${parameter.Name.Text}";
        return new ParameterName() { Text = ParameterBlockText };
    }

    private void AddMethodLocalStates(VerificationContext verificationContext)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;
        AliasTable AliasTable = verificationContext.AliasTable;

        foreach (KeyValuePair<LocalName, Local> Entry in HostMethod.LocalTable)
        {
            Local Local = Entry.Value;
            LocalName LocalBlockName = CreateLocalBlockName(HostMethod, Local);
            Variable LocalVariable = new(LocalBlockName, Local.Type);

            AliasTable.AddOrIncrement(LocalVariable);

            VariableAlias LocalBlockAlias = AliasTable.GetAlias(LocalVariable);

            Expr LocalExpr = CreateVariableExpr(verificationContext, LocalBlockAlias.ToString(), Local.Type);
            Expr InitializerExpr = CreateInitializerExpr(Local);
            BoolExpr InitExpr = Context.MkEq(LocalExpr, InitializerExpr);

            Log($"Adding {InitExpr}");
            verificationContext.Solver.Assert(InitExpr);
        }
    }

    private static LocalName CreateLocalBlockName(Method method, Local local)
    {
        string LocalBlockText = $"{method.Name.Text}${local.Name.Text}";
        return new LocalName() { Text = LocalBlockText };
    }

    // checkOpposite: false for a call outside the call (the call is assumed to fulfill the contract)
    //                true for a call from within the class (the call must fulfill the contract)
    private bool AddMethodRequires(VerificationContext verificationContext, bool checkOpposite)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;

        for (int i = 0; i < HostMethod.RequireList.Count; i++)
        {
            Require Require = HostMethod.RequireList[i];
            IExpression AssertionExpression = Require.BooleanExpression;
            VerificationErrorType ErrorType = VerificationErrorType.RequireError;

            if (!BuildExpression(verificationContext, AssertionExpression, out BoolExpr AssertionExpr))
                return false;

            if (checkOpposite && !AddMethodAssertionOpposite(verificationContext, AssertionExpr, i, AssertionExpression.ToString(), ErrorType))
                return false;

            if (!AddMethodAssertionNormal(verificationContext, AssertionExpr, i, AssertionExpression.ToString(), ErrorType, keepNormal: true))
                return false;
        }

        return true;
    }

    private bool AddMethodEnsures(VerificationContext verificationContext, bool keepNormal)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;

        for (int i = 0; i < HostMethod.EnsureList.Count; i++)
        {
            Ensure Ensure = HostMethod.EnsureList[i];
            IExpression AssertionExpression = Ensure.BooleanExpression;
            VerificationErrorType ErrorType = VerificationErrorType.EnsureError;

            if (!BuildExpression(verificationContext, AssertionExpression, out BoolExpr AssertionExpr))
                return false;

            if (!AddMethodAssertionNormal(verificationContext, AssertionExpr, i, AssertionExpression.ToString(), ErrorType, keepNormal))
                return false;

            if (!AddMethodAssertionOpposite(verificationContext, AssertionExpr, i, AssertionExpression.ToString(), ErrorType))
                return false;
        }

        return true;
    }

    // keepNormal: true if we keep the contract (for all require, and for ensure after a call from within the class, since we must fulfill the contract. From outside the class, we no longer care)
    private bool AddMethodAssertionNormal(VerificationContext verificationContext, BoolExpr assertionExpr, int index, string text, VerificationErrorType errorType, bool keepNormal)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;
        string AssertionType = VerificationErrorTypeToText(errorType);
        bool Result = true;

        if (!keepNormal)
            verificationContext.Solver.Push();

        Log($"Adding {AssertionType} {assertionExpr}");
        verificationContext.Solver.Assert(assertionExpr);

        if (verificationContext.Solver.Check() != Status.SATISFIABLE)
        {
            Log($"Inconsistent {AssertionType} state for class {ClassName}");
            VerificationResult = VerificationResult.Default with { ErrorType = errorType, ClassName = ClassName, MethodName = HostMethod.Name.Text, ErrorIndex = index, ErrorText = text };

            Result = false;
        }

        if (!keepNormal)
            verificationContext.Solver.Pop();

        return Result;
    }

    private bool AddMethodAssertionOpposite(VerificationContext verificationContext, BoolExpr assertionExpr, int index, string text, VerificationErrorType errorType)
    {
        Method? HostMethod = verificationContext.HostMethod;
        string AssertionType = VerificationErrorTypeToText(errorType);
        bool Result = true;
        BoolExpr AssertionOppositeExpr = Context.MkNot(assertionExpr);

        verificationContext.Solver.Push();

        Log($"Adding {AssertionType} opposite {AssertionOppositeExpr}");
        verificationContext.Solver.Assert(AssertionOppositeExpr);

        if (verificationContext.Solver.Check() == Status.SATISFIABLE)
        {
            Log($"Violation of {AssertionType} for class {ClassName}");

            string MethodName = HostMethod is not null ? HostMethod.Name.Text : string.Empty;
            VerificationResult = VerificationResult.Default with { ErrorType = errorType, ClassName = ClassName, MethodName = MethodName, ErrorIndex = index, ErrorText = text };

            string ModelString = TextBuilder.Normalized(verificationContext.Solver.Model.ToString());
            Log(ModelString);

            Result = false;
        }

        verificationContext.Solver.Pop();

        return Result;
    }

    private string VerificationErrorTypeToText(VerificationErrorType errorType)
    {
        Dictionary<VerificationErrorType, string> AssertionTypeTable = new()
        {
            { VerificationErrorType.RequireError, "require" },
            { VerificationErrorType.EnsureError, "ensure" },
            { VerificationErrorType.AssumeError, "assume" },
        };

        Debug.Assert(AssertionTypeTable.ContainsKey(errorType));

        return AssertionTypeTable[errorType];
    }

    private void AddToSolver(VerificationContext verificationContext, BoolExpr boolExpr)
    {
        if (verificationContext.Branch is BoolExpr Branch)
        {
            verificationContext.Solver.Assert(Context.MkImplies(Branch, boolExpr));
            Log($"Adding {Branch} => {boolExpr}");
        }
        else
        {
            verificationContext.Solver.Assert(boolExpr);
            Log($"Adding {boolExpr}");
        }
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
    private Stopwatch VerificationWatch = new Stopwatch();
}
