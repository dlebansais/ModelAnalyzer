namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// <param name="className">The class name of the root object.</param>
    public Verifier(string className)
    {
        // TODO: make ClassName a required property.
        ClassName = className;
        Context = new();
    }

    /// <summary>
    /// Verifies the code.
    /// </summary>
    public void Verify()
    {
        VerificationWatch.Restart();

        Context.Logger = Logger;
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
    public string ClassName { get; }

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
    /// Gets or sets the logger.
    /// </summary>
    public IAnalysisLogger Logger { get; set; } = new NullLogger();

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
        using Solver Solver = Context.CreateSolver();
        ObjectManager ObjectManager = new(Context) { Solver = Solver, ClassModelTable = ClassModelTable };
        VerificationContext VerificationContext = new() { Solver = Solver, ClassModelTable = ClassModelTable, PropertyTable = PropertyTable, FieldTable = FieldTable, MethodTable = MethodTable, ObjectManager = ObjectManager };

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

        foreach (KeyValuePair<PropertyName, Property> Entry in verificationContext.PropertyTable)
        {
            Property Property = Entry.Value;
            verificationContext.ObjectManager.CreateVariable(verificationContext.ObjectManager.ThisObject, hostMethod: null, Property.Name, Property.Type, Property.Initializer, initWithDefault: true);
        }

        foreach (KeyValuePair<FieldName, Field> Entry in verificationContext.FieldTable)
        {
            Field Field = Entry.Value;
            verificationContext.ObjectManager.CreateVariable(verificationContext.ObjectManager.ThisObject, hostMethod: null, Field.Name, Field.Type, Field.Initializer, initWithDefault: true);
        }
    }

    private Local FindOrCreateResultLocal(VerificationContext verificationContext, ExpressionType returnType)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;
        Local ResultLocal = verificationContext.ObjectManager.FindOrCreateResultLocal(HostMethod, returnType);

        return ResultLocal;
    }

    private bool AddClassInvariant(VerificationContext verificationContext)
    {
        bool Result = true;

        Log($"Invariant for class {ClassName}");

        for (int i = 0; i < InvariantList.Count && Result == true; i++)
        {
            Invariant Invariant = InvariantList[i];

            if (BuildExpression(verificationContext, Invariant.BooleanExpression, out IExprSet<IBoolExprCapsule> InvariantExpression))
            {
                IExprSet<IBoolExprCapsule> InvariantOpposite = Context.CreateOppositeExprSet(InvariantExpression);

                verificationContext.Solver.Push();

                foreach (IBoolExprCapsule Expression in InvariantOpposite.Expressions)
                {
                    Log($"Adding invariant opposite {Expression.Item}");
                    verificationContext.Solver.Assert(Expression.Item);
                }

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

        foreach (KeyValuePair<ParameterName, Parameter> Entry in HostMethod.ParameterTable)
        {
            Parameter Parameter = Entry.Value;
            verificationContext.ObjectManager.CreateVariable(owner: null, HostMethod, Parameter.Name, Parameter.Type, variableInitializer: null, initWithDefault: false);
        }
    }

    private void AddMethodLocalStates(VerificationContext verificationContext)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;

        foreach (KeyValuePair<LocalName, Local> Entry in HostMethod.LocalTable)
        {
            Local Local = Entry.Value;
            verificationContext.ObjectManager.CreateVariable(owner: null, HostMethod, Local.Name, Local.Type, Local.Initializer, initWithDefault: true);
        }
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

            if (!BuildExpression(verificationContext, AssertionExpression, out IExprSet<IBoolExprCapsule> AssertionExpr))
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

            if (!BuildExpression(verificationContext, AssertionExpression, out IExprSet<IBoolExprCapsule> AssertionExpr))
                return false;

            if (!AddMethodAssertionNormal(verificationContext, AssertionExpr, i, AssertionExpression.ToString(), ErrorType, keepNormal))
                return false;

            if (!AddMethodAssertionOpposite(verificationContext, AssertionExpr, i, AssertionExpression.ToString(), ErrorType))
                return false;
        }

        return true;
    }

    // keepNormal: true if we keep the contract (for all require, and for ensure after a call from within the class, since we must fulfill the contract. From outside the class, we no longer care)
    private bool AddMethodAssertionNormal(VerificationContext verificationContext, IExprSet<IBoolExprCapsule> assertionExpr, int index, string text, VerificationErrorType errorType, bool keepNormal)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;
        string AssertionType = VerificationErrorTypeToText(errorType);
        bool Result = true;

        if (!keepNormal)
            verificationContext.Solver.Push();

        foreach (IBoolExprCapsule Expression in assertionExpr.Expressions)
        {
            Log($"Adding {AssertionType} {Expression.Item}");
            verificationContext.Solver.Assert(Expression.Item);
        }

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

    private bool AddMethodAssertionOpposite(VerificationContext verificationContext, IExprSet<IBoolExprCapsule> assertionExpr, int index, string text, VerificationErrorType errorType)
    {
        Method? HostMethod = verificationContext.HostMethod;
        string AssertionType = VerificationErrorTypeToText(errorType);
        bool Result = true;
        IExprSet<IBoolExprCapsule> AssertionOppositeExpr = Context.CreateOppositeExprSet(assertionExpr);

        verificationContext.Solver.Push();

        foreach (IBoolExprCapsule Expression in AssertionOppositeExpr.Expressions)
        {
            Log($"Adding {AssertionType} opposite {Expression.Item}");
            verificationContext.Solver.Assert(Expression.Item);
        }

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

    private void Log(string message)
    {
        Debug.WriteLine(message);

        Logger.Log(message);
    }

    private SolverContext Context;
    private Stopwatch VerificationWatch = new Stopwatch();
}
