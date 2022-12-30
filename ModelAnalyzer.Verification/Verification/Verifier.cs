namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
        if (!AddMethodRequires(solver, aliasTable, method))
            return false;

        BoolExpr MainBranch = Context.MkBool(true);
        Field? ResultField = null;

        if (!AddStatementListExecution(solver, aliasTable, method, ref ResultField, MainBranch, method.StatementList))
            return false;

        if (!AddMethodEnsures(solver, aliasTable, method, ResultField))
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

    private static Parameter CreateParameterLocal(Method method, Parameter parameter)
    {
        ParameterName ParameterLocalName = CreateParameterLocalName(method, parameter);
        ExpressionType ParameterLocalType = parameter.Type;
        Parameter ParameterLocal = new Parameter() { Name = ParameterLocalName, Type = ParameterLocalType };

        return ParameterLocal;
    }

    private static ParameterName CreateParameterLocalName(Method method, Parameter parameter)
    {
        string ParameterLocalText = GetParameterLocalText(method, parameter);
        return new ParameterName() { Text = ParameterLocalText };
    }

    private static string GetParameterLocalText(Method method, Parameter parameter)
    {
        return $"{method.Name.Text}${parameter.Name.Text}";
    }

    private bool AddMethodRequires(Solver solver, AliasTable aliasTable, Method method)
    {
        for (int i = 0; i < method.RequireList.Count; i++)
        {
            Require Require = method.RequireList[i];
            BoolExpr RequireExpr = BuildExpression<BoolExpr>(aliasTable, method, resultField: null, Require.BooleanExpression);

            Log($"Adding {RequireExpr}");
            solver.Assert(RequireExpr);

            if (solver.Check() != Status.SATISFIABLE)
            {
                Log($"Inconsistent require state for class {ClassName}");
                VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.RequireError, ClassName = ClassName, MethodName = method.Name.Text, ErrorIndex = i };
                return false;
            }
        }

        return true;
    }

    private bool AddMethodEnsures(Solver solver, AliasTable aliasTable, Method method, Field? resultField)
    {
        bool Result = true;

        for (int i = 0; i < method.EnsureList.Count && Result == true; i++)
        {
            Ensure Ensure = method.EnsureList[i];
            BoolExpr EnsureExpr = BuildExpression<BoolExpr>(aliasTable, method, resultField, Ensure.BooleanExpression);
            BoolExpr EnsureOppositeExpr = Context.MkNot(EnsureExpr);

            solver.Push();

            Log($"Adding ensure opposite {EnsureOppositeExpr}");
            solver.Assert(EnsureOppositeExpr);

            if (solver.Check() == Status.SATISFIABLE)
            {
                Log($"Ensure violation for class {ClassName}");
                VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.EnsureError, ClassName = ClassName, MethodName = method.Name.Text, ErrorIndex = i };

                string ModelString = TextBuilder.Normalized(solver.Model.ToString());
                Log(ModelString);

                Result = false;
            }

            solver.Pop();
        }

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

    /// <summary>
    /// Gets a variable from its name.
    /// </summary>
    /// <param name="fieldTable">The table of fields.</param>
    /// <param name="hostMethod">The host method, null in invariants.</param>
    /// <param name="resultField">The optional result field.</param>
    /// <param name="variableName">The variable name.</param>
    public static Variable GetVariable(ReadOnlyFieldTable fieldTable, Method? hostMethod, Field? resultField, IVariableName variableName)
    {
        Variable? Result = null;

        foreach (KeyValuePair<FieldName, Field> Entry in fieldTable)
            if (Entry.Key.Text == variableName.Text)
            {
                Field Field = Entry.Value;
                Result = new Variable(Field.Name, Field.Type);
                break;
            }

        if (hostMethod is not null)
        {
            foreach (KeyValuePair<ParameterName, Parameter> Entry in hostMethod.ParameterTable)
                if (Entry.Key.Text == variableName.Text)
                {
                    Parameter Parameter = Entry.Value;
                    ParameterName ParameterLocalName = CreateParameterLocalName(hostMethod, Parameter);
                    Result = new Variable(ParameterLocalName, Parameter.Type);
                    break;
                }
        }

        if (resultField is not null && resultField.Name.Text == variableName.Text)
        {
            Debug.Assert(Result is null);

            Result = new Variable(resultField.Name, resultField.Type);
        }

        Debug.Assert(Result is not null);

        return Result!;
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
