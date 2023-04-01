namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AnalysisLogger;
using CodeProverBinding;

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
    required public ClassModelTable ClassModelTable { get; init; }

    /// <summary>
    /// Gets the class name.
    /// </summary>
    required public ClassName ClassName { get; init; }

    /// <summary>
    /// Gets the property table.
    /// </summary>
    required public ReadOnlyPropertyTable PropertyTable { get; init; }

    /// <summary>
    /// Gets the field table.
    /// </summary>
    required public ReadOnlyFieldTable FieldTable { get; init; }

    /// <summary>
    /// Gets or sets the method table.
    /// </summary>
    required public ReadOnlyMethodTable MethodTable { get; set; }

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

                if (Method.AccessModifier == AccessModifier.Public && !Method.IsPreloaded)
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
        using Binder Binder = new Binder(Prover.Z3);

        Context = new(Binder);
        Context.Logger = Logger;

        ObjectManager ObjectManager = new(Context)
        {
            ClassModelTable = ClassModelTable,
        };

        ClassModel ClassModel = ClassModelTable[ClassName];
        Instance ThisInstance = new() { ClassModel = ClassModel, Expr = ObjectManager.RootInstanceExpr };

        VerificationContext VerificationContext = new()
        {
            Binder = Binder,
            ClassModelTable = ClassModelTable,
            ObjectManager = ObjectManager,
            Instance = ThisInstance,
        };

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
            CreateVariable(verificationContext, verificationContext.Instance, hostMethod: null, Property, Property.Initializer, initWithDefault: true);
        }

        foreach (KeyValuePair<FieldName, Field> Entry in verificationContext.FieldTable)
        {
            Field Field = Entry.Value;
            CreateVariable(verificationContext, verificationContext.Instance, hostMethod: null, Field, Field.Initializer, initWithDefault: true);
        }
    }

    private Local? FindOrCreateResultLocal(VerificationContext verificationContext, ExpressionType returnType)
    {
        if (returnType != ExpressionType.Void)
        {
            Debug.Assert(verificationContext.HostMethod is not null);

            Method HostMethod = verificationContext.HostMethod!;
            Local ResultLocal = verificationContext.ObjectManager.FindOrCreateResultLocal(HostMethod, returnType);

            return ResultLocal;
        }
        else
            return null;
    }

    private bool AddClassInvariant(VerificationContext verificationContext)
    {
        bool Result = true;
        Instance Instance = verificationContext.Instance;
        ClassName ClassName = Instance.ClassModel.ClassName;
        IReadOnlyList<Invariant> InvariantList = Instance.ClassModel.InvariantList;

        Log($"Invariant for class {ClassName}");

        for (int i = 0; i < InvariantList.Count && Result == true; i++)
        {
            Invariant Invariant = InvariantList[i];

            if (BuildExpression(verificationContext, Invariant.BooleanExpression, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> NewExpr))
            {
                Debug.Assert(NewExpr is IExprSingle<IBoolExprCapsule>);

                IExprSingle<IBoolExprCapsule> InvariantExpr = (IExprSingle<IBoolExprCapsule>)NewExpr;
                IExprSingle<IBoolExprCapsule> InvariantOppositeExpr = Context.CreateOppositeExprSet(InvariantExpr);

                verificationContext.Binder.SaveProverState(CodeProverBinding.CorrectnessCheckType.NotSatisfiable);

                foreach (IBoolExprCapsule Expression in InvariantOppositeExpr.AllExpressions)
                {
                    Log($"Adding invariant opposite {Expression.Item}");
                    Expression.Item.Assert();
                }

                verificationContext.Binder.CheckCorrectness();
                if (!verificationContext.Binder.IsCorrect)
                {
                    Log($"Invariant violation for class {ClassName}");
                    VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.InvariantError, ClassName = ClassName, MethodName = string.Empty, LocationId = ((Expression)Invariant.BooleanExpression).LocationId };

                    string ModelString = verificationContext.Binder.SatisfiedModel;
                    Log(ModelString);

                    Result = false;
                }

                verificationContext.Binder.RestoreProverState();
            }
            else
                Result = false;
        }

        return Result;
    }

    private bool AddMethodCallStateWithInit(VerificationContext verificationContext, Method hostMethod)
    {
        VerificationContext LocalVerificationContext = verificationContext with { HostMethod = hostMethod, HostBlock = hostMethod.RootBlock };
        Variable? ResultVariable = FindOrCreateResultLocal(LocalVerificationContext, hostMethod.ReturnType);
        VerificationContext StatementVerificationContext = LocalVerificationContext with { ResultVariable = ResultVariable };

        return AddMethodCallState(StatementVerificationContext);
    }

    private bool AddMethodCallState(VerificationContext verificationContext)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;

        AddMethodParameterStates(verificationContext);
        if (!AddMethodRequires(verificationContext, ClassName.Empty, LocationId.None, checkOpposite: false))
            return false;

        AddMethodLocalStates(verificationContext);

        if (!AddStatementListExecution(verificationContext, HostMethod.RootBlock))
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
            CreateVariable(verificationContext, owner: null, HostMethod, Parameter, variableInitializer: null, initWithDefault: false);
        }
    }

    private void AddMethodLocalStates(VerificationContext verificationContext)
    {
        Debug.Assert(verificationContext.HostMethod is not null);
        Method HostMethod = verificationContext.HostMethod!;

        foreach (KeyValuePair<LocalName, Local> Entry in HostMethod.RootBlock.LocalTable)
        {
            Local Local = Entry.Value;
            CreateVariable(verificationContext, owner: null, HostMethod, Local, Local.Initializer, initWithDefault: true);
        }
    }

    // locationId:    location of the call site
    // checkOpposite: false for a call outside the call (the call is assumed to fulfill the contract)
    //                true for a call from within the class (the call must fulfill the contract)
    private bool AddMethodRequires(VerificationContext verificationContext, ClassName locationClassName, LocationId locationId, bool checkOpposite)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;

        for (int i = 0; i < HostMethod.RequireList.Count; i++)
        {
            Require Require = HostMethod.RequireList[i];
            Expression AssertionExpression = (Expression)Require.BooleanExpression;
            VerificationErrorType ErrorType = VerificationErrorType.RequireError;
            ClassName LocationClassName = locationClassName != ClassName.Empty ? locationClassName : ClassName;
            LocationId RequireLocationId = locationId != LocationId.None ? locationId : AssertionExpression.LocationId;

            if (!BuildExpression(verificationContext, AssertionExpression, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> NewExpr))
                return false;

            Debug.Assert(NewExpr is IExprSingle<IBoolExprCapsule>);

            IExprSingle<IBoolExprCapsule> AssertionExpr = (IExprSingle<IBoolExprCapsule>)NewExpr;
            if (checkOpposite && !AddMethodAssertionOpposite(verificationContext, AssertionExpr, LocationClassName, RequireLocationId, AssertionExpression.ToString(), ErrorType))
                return false;

            if (!AddMethodAssertionNormal(verificationContext, AssertionExpr, LocationClassName, RequireLocationId, AssertionExpression.ToString(), ErrorType, keepNormal: true))
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
            Expression AssertionExpression = (Expression)Ensure.BooleanExpression;
            VerificationErrorType ErrorType = VerificationErrorType.EnsureError;

            if (!BuildExpression(verificationContext, AssertionExpression, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> NewExpr))
                return false;

            Debug.Assert(NewExpr is IExprSingle<IBoolExprCapsule>);

            IExprSingle<IBoolExprCapsule> AssertionExpr = (IExprSingle<IBoolExprCapsule>)NewExpr;
            if (!AddMethodAssertionNormal(verificationContext, AssertionExpr, ClassName.Empty, AssertionExpression.LocationId, AssertionExpression.ToString(), ErrorType, keepNormal))
                return false;

            if (!AddMethodAssertionOpposite(verificationContext, AssertionExpr, ClassName.Empty, AssertionExpression.LocationId, AssertionExpression.ToString(), ErrorType))
                return false;
        }

        return true;
    }

    // keepNormal: true if we keep the contract (for all require, and for ensure after a call from within the class, since we must fulfill the contract. From outside the class, we no longer care)
    private bool AddMethodAssertionNormal(VerificationContext verificationContext, IExprSingle<IBoolExprCapsule> assertionExpr, ClassName locationClassName, LocationId locationId, string errorText, VerificationErrorType errorType, bool keepNormal)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;
        string AssertionType = VerificationErrorTypeToText(errorType);
        bool Result = true;

        if (!keepNormal)
            verificationContext.Binder.SaveProverState(CorrectnessCheckType.Satisfiable);

        Log($"Adding {AssertionType} {assertionExpr.MainExpression.Item}");
        assertionExpr.MainExpression.Item.Assert();

        verificationContext.Binder.CheckCorrectness();
        if (!verificationContext.Binder.IsCorrect)
        {
            Log($"Inconsistent {AssertionType} state for class {ClassName}");

            ClassName AssertionClassName = locationClassName != ClassName.Empty ? locationClassName : ClassName;
            string AssertionMethodName = HostMethod.Name.Text;

            VerificationResult = VerificationResult.Default with
            {
                ErrorType = errorType,
                ClassName = AssertionClassName,
                MethodName = AssertionMethodName,
                LocationId = locationId,
                ErrorText = errorText,
            };

            Result = false;
        }

        if (!keepNormal)
            verificationContext.Binder.RestoreProverState();

        return Result;
    }

    private bool AddMethodAssertionOpposite(VerificationContext verificationContext, IExprSingle<IBoolExprCapsule> assertionExpr, ClassName locationClassName, LocationId locationId, string errorText, VerificationErrorType errorType)
    {
        Method? HostMethod = verificationContext.HostMethod;
        string AssertionType = VerificationErrorTypeToText(errorType);
        bool Result = true;
        IExprSingle<IBoolExprCapsule> AssertionOppositeExpr = Context.CreateOppositeExprSet(assertionExpr);

        verificationContext.Binder.SaveProverState(CorrectnessCheckType.NotSatisfiable);

        Log($"Adding {AssertionType} opposite {AssertionOppositeExpr.MainExpression.Item}");
        AssertionOppositeExpr.MainExpression.Item.Assert();

        verificationContext.Binder.CheckCorrectness();
        if (!verificationContext.Binder.IsCorrect)
        {
            Log($"Violation of {AssertionType} for class {ClassName}");

            ClassName AssertionClassName = locationClassName != ClassName.Empty ? locationClassName : (HostMethod is not null ? HostMethod.ClassName : ClassName);
            string AssertionMethodName = HostMethod is not null ? HostMethod.Name.Text : string.Empty;

            VerificationResult = VerificationResult.Default with
            {
                ErrorType = errorType,
                ClassName = AssertionClassName,
                MethodName = AssertionMethodName,
                LocationId = locationId,
                ErrorText = errorText,
            };

            string ModelString = verificationContext.Binder.SatisfiedModel;
            Log(ModelString);

            Result = false;
        }

        verificationContext.Binder.RestoreProverState();

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

    public static ClassModel GetLastClassModel(VerificationContext verificationContext, List<IVariable> variablePath)
    {
        Debug.Assert(variablePath.Count >= 1);

        IVariable LastVariable = variablePath.Last();
        ExpressionType VariableType = LastVariable.Type;

        return GetClassModel(verificationContext, VariableType.TypeName);
    }

    public static ClassModel GetClassModel(VerificationContext verificationContext, ClassName className)
    {
        ClassModelTable ClassModelTable = verificationContext.ClassModelTable;

        Debug.Assert(ClassModelTable.ContainsKey(className));

        return ClassModelTable[className];
    }

    private IExprBase<IExprCapsule, IExprCapsule> CreateVariable(VerificationContext verificationContext, Instance? owner, Method? hostMethod, IVariable variable, ILiteralExpression? variableInitializer, bool initWithDefault)
    {
        IExprBase<IExprCapsule, IExprCapsule> Result = verificationContext.ObjectManager.CreateVariable(owner, hostMethod, variable, variableInitializer, initWithDefault);

        if (variable.Type.IsArray)
        {
            Method GetterMethod = verificationContext.ObjectManager.GetArrayGetter(owner, hostMethod, variable);
            AddArrayGetterMethod(GetterMethod);
        }

        return Result;
    }

    private IExprBase<IExprCapsule, IExprCapsule> CreateVariable(VerificationContext verificationContext, Instance? owner, Method? hostMethod, IVariable variable, IBoolExprCapsule? branch, IExprBase<IExprCapsule, IExprCapsule>? initializerExpr)
    {
        IExprBase<IExprCapsule, IExprCapsule> Result = verificationContext.ObjectManager.CreateVariable(owner, hostMethod, variable, branch, initializerExpr);

        if (variable.Type.IsArray)
        {
            Method GetterMethod = verificationContext.ObjectManager.GetArrayGetter(owner, hostMethod, variable);
            AddArrayGetterMethod(GetterMethod);
        }

        return Result;
    }

    private void AddArrayGetterMethod(Method method)
    {
        MethodTable NewMethodTable = new();
        foreach (KeyValuePair<MethodName, Method> Entry in MethodTable)
            NewMethodTable.AddItem(Entry.Value);

        NewMethodTable.AddItem(method);

        MethodTable = NewMethodTable.AsReadOnly();
    }

    private void Log(string message)
    {
        Debug.WriteLine(message);

        Logger.Log(message);
    }

    private SolverContext Context = null!;
    private Stopwatch VerificationWatch = new Stopwatch();
}
