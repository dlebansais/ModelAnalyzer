﻿namespace DemoAnalyzer;

using Microsoft.Z3;
using System;
using System.Collections.Generic;

public class Verifier : IDisposable
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

    public required int MaxDepth { get; init; }
    public required string ClassName { get; init; }
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

        if (CallSequenceString == string.Empty)
            Logger.Log($"Call sequence empty");
        else
            Logger.Log($"Call sequence: {CallSequenceString}");

        foreach (Method Method in callSequence)
            AddMethodCallState(solver, aliasTable, Method);

        AddClassInvariant(solver, aliasTable);

        if (solver.Check() == Status.SATISFIABLE)
        {
            IsInvariantViolated = true;
            Logger.Log($"Invariant violation for class {ClassName}");

            string ModelString = solver.Model.ToString();
            ModelString = ModelString.Replace("\n", "\r\n");
            Logger.Log(ModelString);
        }
        else
            Logger.Log($"Invariant preserved for class {ClassName}");
    }

    private void AddInitialState(Solver solver, AliasTable aliasTable)
    {
        Logger.Log($"Initial state for class {ClassName}");

        foreach (KeyValuePair<FieldName, IField> Entry in FieldTable)
        {
            string FieldName = Entry.Key.Name;
            aliasTable.AddName(FieldName);
            string FieldNameAlias = aliasTable.GetAlias(FieldName);

            IntExpr FieldExpr = ctx.MkIntConst(FieldNameAlias);
            BoolExpr InitExpr = ctx.MkEq(FieldExpr, Zero);

            Logger.Log($"Adding {InitExpr}");
            solver.Assert(InitExpr);
        }
    }

    private void AddClassInvariant(Solver solver, AliasTable aliasTable)
    {
        Logger.Log($"Invariant for class {ClassName}");

        List<BoolExpr> AssertionList = new();

        foreach (IInvariant Item in InvariantList)
            if (Item is Invariant Invariant)
            {
                BoolExpr InvariantExpression = BuildExpression<BoolExpr>(aliasTable, Invariant.BooleanExpression);
                AssertionList.Add(InvariantExpression);
            }

        BoolExpr AllInvariants = ctx.MkAnd(AssertionList);
        BoolExpr AllInvariantsOpposite = ctx.MkNot(AllInvariants);

        Logger.Log($"Adding invariant opposite {AllInvariantsOpposite}");
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

        AddToSolver(solver, branch, ctx.MkEq(DestinationExpr, SourceExpr));
    }

    private void AddConditionalExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, ConditionalStatement conditionalStatement)
    {
        BoolExpr ConditionExpr = BuildExpression<BoolExpr>(aliasTable, conditionalStatement.Condition);
        BoolExpr TrueBranchExpr = ctx.MkAnd(branch, ConditionExpr);
        BoolExpr FalseBranchExpr = ctx.MkAnd(branch, ctx.MkNot(ConditionExpr));

        AliasTable BeforeWhenTrue = aliasTable.Clone();
        AddStatementListExecution(solver, aliasTable, TrueBranchExpr, conditionalStatement.WhenTrueStatementList);
        List<string> AliasesOnlyWhenTrue = aliasTable.GetAliasDifference(BeforeWhenTrue);

        AliasTable WhenTrueAliasTable = aliasTable.Clone();

        AliasTable BeforeWhenFalse = WhenTrueAliasTable;
        AddStatementListExecution(solver, aliasTable, FalseBranchExpr, conditionalStatement.WhenFalseStatementList);
        List<string> AliasesOnlyWhenFalse = aliasTable.GetAliasDifference(BeforeWhenFalse);

        AliasTable WhenFalseAliasTable = aliasTable.Clone();

        aliasTable.Merge(WhenTrueAliasTable, out List<string> UpdatedNameList);

        foreach (string NameAlias in AliasesOnlyWhenFalse)
        {
            IntExpr AliasExpr = ctx.MkIntConst(NameAlias);
            BoolExpr InitExpr = ctx.MkEq(AliasExpr, Zero);
            AddToSolver(solver, TrueBranchExpr, InitExpr);
        }

        foreach (string NameAlias in AliasesOnlyWhenTrue)
        {
            IntExpr AliasExpr = ctx.MkIntConst(NameAlias);
            BoolExpr InitExpr = ctx.MkEq(AliasExpr, Zero);
            AddToSolver(solver, FalseBranchExpr, InitExpr);
        }

        foreach (string Name in UpdatedNameList)
        {
            string NameAlias = aliasTable.GetAlias(Name);
            IntExpr DestinationExpr = ctx.MkIntConst(NameAlias);

            string WhenTrueNameAlias = WhenTrueAliasTable.GetAlias(Name);
            IntExpr WhenTrueSourceExpr = ctx.MkIntConst(WhenTrueNameAlias);
            BoolExpr WhenTrueInitExpr = ctx.MkEq(DestinationExpr, WhenTrueSourceExpr);

            string WhenFalseNameAlias = WhenFalseAliasTable.GetAlias(Name);
            IntExpr WhenFalseSourceExpr = ctx.MkIntConst(WhenFalseNameAlias);
            BoolExpr WhenFalseInitExpr = ctx.MkEq(DestinationExpr, WhenFalseSourceExpr);

            AddToSolver(solver, TrueBranchExpr, WhenTrueInitExpr);
            AddToSolver(solver, FalseBranchExpr, WhenFalseInitExpr);
        }
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

    private void AddToSolver(Solver solver, BoolExpr branch, BoolExpr boolExpr)
    {
        solver.Assert(ctx.MkImplies(branch, boolExpr));
        Logger.Log($"Adding {branch} => {boolExpr}");
    }

    public void Dispose()
    {
        ctx.Dispose();
    }

    private Context ctx;
    private IntExpr Zero;
}
