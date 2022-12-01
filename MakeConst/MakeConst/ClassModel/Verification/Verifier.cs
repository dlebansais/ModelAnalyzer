namespace DemoAnalyzer;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Z3;
using System;
using System.Collections.Generic;

public class Verifier : IDisposable
{
    public Verifier()
    {
        // Need model generation turned on.
        ctx = new Context(new Dictionary<string, string>() { { "model", "true" } });
    }

    public void Verify()
    {
        Verify(0);
    }

    public required int MaxDepth { get; init; }
    public required Dictionary<FieldName, IField> FieldTable { get; init; }
    public required Dictionary<MethodName, IMethod> MethodTable { get; init; }
    public required List<IInvariant> InvariantList { get; init; }

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

        Logger.Log($"Call sequence: {CallSequenceString}");

        foreach (Method Method in callSequence)
            AddMethodCallState(solver, aliasTable, Method);

        AddClassInvariant(solver, aliasTable);

        if (solver.Check() == Status.SATISFIABLE)
            IsInvariantViolated = true;

        Logger.Log($"Solver result: {(IsInvariantViolated ? "NOK" : "OK")}");
    }

    private void AddInitialState(Solver solver, AliasTable aliasTable)
    {
        Logger.Log($"Initial state");

        IntExpr Zero = ctx.MkInt(0);

        foreach (KeyValuePair<FieldName, IField> Entry in FieldTable)
        {
            string FieldName = Entry.Key.Name;
            aliasTable.AddName(FieldName);
            string FieldNameAlias = aliasTable.GetAlias(FieldName);

            IntExpr FieldExpr = ctx.MkIntConst(FieldNameAlias);
            AddToSolver(solver, ctx.MkEq(FieldExpr, Zero));
        }
    }

    private void AddClassInvariant(Solver solver, AliasTable aliasTable)
    {
        Logger.Log($"Class invariant");

        List<BoolExpr> AssertionList = new();

        foreach (IInvariant Item in InvariantList)
            if (Item is Invariant Invariant)
            {
                BoolExpr InvariantExpression = BuildExpression<BoolExpr>(aliasTable, Invariant.BooleanExpression);
                AssertionList.Add(ctx.MkNot(InvariantExpression));
            }

        AddToSolver(solver, ctx.MkOr(AssertionList));
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

        BoolExpr True = ctx.MkBool(true);

        AddStatementListExecution(solver, aliasTable, True, method.StatementList);
    }

    private void AddStatementListExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, List<IStatement> statementList)
    {
        foreach (IStatement Statement in statementList)
            switch (Statement)
            {
                case AssignmentStatement Assignment:
                    AddAssignmentExecution(solver, aliasTable, branch, Assignment);
                    break;
                case ConditionalStatement Conditional:
                    AddConditionalExecution(solver, aliasTable, branch, Conditional);
                    break;
                case ReturnStatement Return:
                    AddReturnExecution(solver, aliasTable, branch, Return);
                    break;
                case UnsupportedStatement:
                default:
                    throw new InvalidOperationException("Unexpected unsupported statement");
            }
    }

    private void AddAssignmentExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, AssignmentStatement assignmentStatement)
    {
        Expr SourceExpr = BuildExpression<Expr>(aliasTable, assignmentStatement.Expression);

        string DestinationName = assignmentStatement.Destination.Name;
        aliasTable.IncrementNameAlias(DestinationName);
        string DestinationNameAlias = aliasTable.GetAlias(DestinationName);
        IntExpr DestinationExpr = ctx.MkIntConst(DestinationNameAlias);

        AddToSolver(solver, ctx.MkAnd(branch, ctx.MkEq(DestinationExpr, SourceExpr)));
    }

    private void AddConditionalExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, ConditionalStatement conditionalStatement)
    {
        BoolExpr ConditionExpr = BuildExpression<BoolExpr>(aliasTable, conditionalStatement.Condition);
        AddStatementListExecution(solver, aliasTable, ctx.MkAnd(branch, ConditionExpr), conditionalStatement.WhenTrueStatementList);
        AddStatementListExecution(solver, aliasTable, ctx.MkAnd(branch, ctx.MkNot(ConditionExpr)), conditionalStatement.WhenFalseStatementList);
    }

    private void AddReturnExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, ReturnStatement returnStatement)
    {
    }

    private T BuildExpression<T>(AliasTable aliasTable, IExpression expression)
        where T : Expr
    {
        Expr Result;

        switch (expression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                Result = BuildBinaryExpression(aliasTable, BinaryArithmetic);
                break;
            case BinaryLogicalExpression BinaryLogical:
                Result = BuildBinaryLogicalExpression(aliasTable, BinaryLogical);
                break;
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(aliasTable, Comparison);
                break;
            case LiteralValueExpression LiteralValue:
                Result = BuildLiteralValueExpression(LiteralValue);
                break;
            case VariableValueExpression VariableValue:
                Result = BuildVariableValueExpression(aliasTable, VariableValue);
                break;
            case UnsupportedExpression:
            default:
                throw new InvalidOperationException("Unexpected unsupported expression");
        }

        return Result as T ?? throw new InvalidOperationException($"Expected expression of type {typeof(T).Name}");
    }

    private ArithExpr BuildBinaryExpression(AliasTable aliasTable, BinaryArithmeticExpression binaryArithmeticExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, binaryArithmeticExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, binaryArithmeticExpression.Right);
        return ClassModel.SupportedArithmeticOperators[binaryArithmeticExpression.OperatorKind].Asserter(ctx, Left, Right);
    }

    private BoolExpr BuildBinaryLogicalExpression(AliasTable aliasTable, BinaryLogicalExpression binaryLogicalExpression)
    {
        BoolExpr Left = BuildExpression<BoolExpr>(aliasTable, binaryLogicalExpression.Left);
        BoolExpr Right = BuildExpression<BoolExpr>(aliasTable, binaryLogicalExpression.Right);
        return ClassModel.SupportedLogicalOperators[binaryLogicalExpression.OperatorKind].Asserter(ctx, Left, Right);
    }

    private BoolExpr BuildComparisonExpression(AliasTable aliasTable, ComparisonExpression comparisonExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, comparisonExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, comparisonExpression.Right);
        return ClassModel.SupportedComparisonOperators[comparisonExpression.OperatorKind].Asserter(ctx, Left, Right);
    }

    private ArithExpr BuildLiteralValueExpression(LiteralValueExpression literalValueExpression)
    {
        return ctx.MkInt(literalValueExpression.Value);
    }

    private ArithExpr BuildVariableValueExpression(AliasTable aliasTable, VariableValueExpression variableValueExpression)
    {
        string VariableName = variableValueExpression.Variable.Name;
        string VariableAliasName = aliasTable.GetAlias(VariableName);

        return ctx.MkIntConst(VariableAliasName);
    }

    private void AddToSolver(Solver solver, BoolExpr boolExpr)
    {
        solver.Assert(boolExpr);
        Logger.Log($"Adding {boolExpr}");
    }

    public void Dispose()
    {
        ctx.Dispose();
    }

    private Context ctx;
}
