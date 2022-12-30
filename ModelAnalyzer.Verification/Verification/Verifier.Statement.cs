namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;

/// <summary>
/// Represents a code verifier.
/// </summary>
internal partial class Verifier : IDisposable
{
    private bool AddStatementListExecution(Solver solver, AliasTable aliasTable, Method hostMethod, ref Field? resultField, BoolExpr branch, List<Statement> statementList)
    {
        foreach (Statement Statement in statementList)
            if (!AddStatementExecution(solver, aliasTable, hostMethod, ref resultField, branch, Statement))
                return false;

        return true;
    }

    private bool AddStatementExecution(Solver solver, AliasTable aliasTable, Method hostMethod, ref Field? resultField, BoolExpr branch, Statement statement)
    {
        bool Result = true;
        bool IsAdded = false;

        switch (statement)
        {
            case AssignmentStatement Assignment:
                AddAssignmentExecution(solver, aliasTable, hostMethod, ref resultField, branch, Assignment);
                IsAdded = true;
                break;
            case ConditionalStatement Conditional:
                Result = AddConditionalExecution(solver, aliasTable, hostMethod, ref resultField, branch, Conditional);
                IsAdded = true;
                break;
            case MethodCallStatement MethodCall:
                Result = AddMethodCallExecution(solver, aliasTable, hostMethod, ref resultField, branch, MethodCall);
                IsAdded = true;
                break;
            case ReturnStatement Return:
                AddReturnExecution(solver, aliasTable, hostMethod, ref resultField, branch, Return);
                IsAdded = true;
                break;
        }

        Debug.Assert(IsAdded);

        return Result;
    }

    private void AddAssignmentExecution(Solver solver, AliasTable aliasTable, Method hostMethod, ref Field? resultField, BoolExpr branch, AssignmentStatement assignmentStatement)
    {
        Expr SourceExpr = BuildExpression<Expr>(aliasTable, hostMethod, resultField, assignmentStatement.Expression);
        Variable Destination = GetVariable(FieldTable, hostMethod, resultField, assignmentStatement.DestinationName);

        aliasTable.IncrementAlias(Destination);
        VariableAlias DestinationNameAlias = aliasTable.GetAlias(Destination);
        Expr DestinationExpr = CreateVariableExpr(DestinationNameAlias.ToString(), assignmentStatement.Expression.GetExpressionType(FieldTable, hostMethod, resultField));

        AddToSolver(solver, branch, Context.MkEq(DestinationExpr, SourceExpr));
    }

    private bool AddConditionalExecution(Solver solver, AliasTable aliasTable, Method hostMethod, ref Field? resultField, BoolExpr branch, ConditionalStatement conditionalStatement)
    {
        BoolExpr ConditionExpr = BuildExpression<BoolExpr>(aliasTable, hostMethod, resultField, conditionalStatement.Condition);
        BoolExpr TrueBranchExpr = Context.MkAnd(branch, ConditionExpr);
        BoolExpr FalseBranchExpr = Context.MkAnd(branch, Context.MkNot(ConditionExpr));

        AliasTable BeforeWhenTrue = aliasTable.Clone();
        bool TrueBranchResult = AddStatementListExecution(solver, aliasTable, hostMethod, ref resultField, TrueBranchExpr, conditionalStatement.WhenTrueStatementList);
        List<VariableAlias> AliasesOnlyWhenTrue = aliasTable.GetAliasDifference(BeforeWhenTrue);

        // For the else branch, start alias indexes from what they are at the end of the if branch.
        AliasTable WhenTrueAliasTable = aliasTable.Clone();

        AliasTable BeforeWhenFalse = WhenTrueAliasTable;
        bool FalseBranchResult = AddStatementListExecution(solver, aliasTable, hostMethod, ref resultField, FalseBranchExpr, conditionalStatement.WhenFalseStatementList);
        List<VariableAlias> AliasesOnlyWhenFalse = aliasTable.GetAliasDifference(BeforeWhenFalse);

        AliasTable WhenFalseAliasTable = aliasTable.Clone();

        // Merge aliases from the if branch (the table currently contains the end of the end branch).
        aliasTable.Merge(WhenTrueAliasTable, out List<Variable> UpdatedNameList);

        AddConditionalAliases(solver, TrueBranchExpr, AliasesOnlyWhenFalse);
        AddConditionalAliases(solver, FalseBranchExpr, AliasesOnlyWhenTrue);

        foreach (Variable Variable in UpdatedNameList)
        {
            ExpressionType VariableType = Variable.Type;

            VariableAlias NameAlias = aliasTable.GetAlias(Variable);
            Expr DestinationExpr = CreateVariableExpr(NameAlias.ToString(), VariableType);

            VariableAlias WhenTrueNameAlias = WhenTrueAliasTable.GetAlias(Variable);
            Expr WhenTrueSourceExpr = CreateVariableExpr(WhenTrueNameAlias.ToString(), VariableType);
            BoolExpr WhenTrueInitExpr = Context.MkEq(DestinationExpr, WhenTrueSourceExpr);

            VariableAlias WhenFalseNameAlias = WhenFalseAliasTable.GetAlias(Variable);
            Expr WhenFalseSourceExpr = CreateVariableExpr(WhenFalseNameAlias.ToString(), VariableType);
            BoolExpr WhenFalseInitExpr = Context.MkEq(DestinationExpr, WhenFalseSourceExpr);

            AddToSolver(solver, TrueBranchExpr, WhenTrueInitExpr);
            AddToSolver(solver, FalseBranchExpr, WhenFalseInitExpr);
        }

        return TrueBranchResult && FalseBranchResult;
    }

    private void AddConditionalAliases(Solver solver, BoolExpr branchExpr, List<VariableAlias> aliasList)
    {
        foreach (VariableAlias Alias in aliasList)
        {
            Variable Variable = Alias.Variable;
            ExpressionType VariableType = Variable.Type;

            Expr FieldExpr = CreateVariableExpr(Alias.ToString(), VariableType);
            Expr InitializerExpr = CreateVariableInitializer(VariableType);
            BoolExpr InitExpr = Context.MkEq(FieldExpr, InitializerExpr);

            AddToSolver(solver, branchExpr, InitExpr);
        }
    }

    private bool AddMethodCallExecution(Solver solver, AliasTable aliasTable, Method hostMethod, ref Field? resultField, BoolExpr branch, MethodCallStatement methodCallStatement)
    {
        bool Result = true;
        bool IsExecuted = false;

        foreach (var Entry in MethodTable)
            if (Entry.Key == methodCallStatement.MethodName)
            {
                Method CalledMethod = Entry.Value;
                Result = AddMethodCallExecution(solver, aliasTable, hostMethod, ref resultField, branch, methodCallStatement, CalledMethod);
                IsExecuted = true;
                break;
            }

        Debug.Assert(IsExecuted);

        return Result;
    }

    private bool AddMethodCallExecution(Solver solver, AliasTable aliasTable, Method hostMethod, ref Field? resultField, BoolExpr branch, MethodCallStatement methodCallStatement, Method calledMethod)
    {
        List<Argument> ArgumentList = methodCallStatement.ArgumentList;

        int Index = 0;
        foreach (var Entry in calledMethod.ParameterTable)
        {
            Argument Argument = ArgumentList[Index++];
            Parameter Parameter = Entry.Value;
            ParameterName ParameterLocalName = CreateParameterLocalName(calledMethod, Parameter);
            Variable ParameterVariable = new(ParameterLocalName, Parameter.Type);

            aliasTable.AddVariable(ParameterVariable);
            VariableAlias FieldNameAlias = aliasTable.GetAlias(ParameterVariable);

            Expr TemporaryLocalExpr = CreateVariableExpr(FieldNameAlias.ToString(), Parameter.Type);
            Expr InitializerExpr = BuildExpression<Expr>(aliasTable, hostMethod, resultField, Argument.Expression);
            BoolExpr InitExpr = Context.MkEq(TemporaryLocalExpr, InitializerExpr);

            AddToSolver(solver, branch, InitExpr);
        }

        if (!AddMethodRequires(solver, aliasTable, calledMethod))
            return false;

        Field? CalledResultField = null;
        if (!AddStatementListExecution(solver, aliasTable, calledMethod, ref CalledResultField, branch, calledMethod.StatementList))
            return false;

        if (!AddMethodEnsures(solver, aliasTable, calledMethod, resultField))
            return false;

        return true;
    }

    private void AddReturnExecution(Solver solver, AliasTable aliasTable, Method hostMethod, ref Field? resultField, BoolExpr branch, ReturnStatement returnStatement)
    {
        Expression? ReturnExpression = (Expression?)returnStatement.Expression;

        if (ReturnExpression is null)
            resultField = null;
        else
        {
            ExpressionType ResultType = ReturnExpression.GetExpressionType(FieldTable, hostMethod, resultField: null);
            resultField = new Field() { Name = new FieldName() { Text = Ensure.ResultKeyword }, Type = ResultType, Initializer = null };

            Expr ResultFieldExpr = CreateVariableExpr(Ensure.ResultKeyword, ResultType);
            Expr ResultInitializerExpr = BuildExpression<Expr>(aliasTable, hostMethod, resultField: null, ReturnExpression);

            AddToSolver(solver, branch, Context.MkEq(ResultFieldExpr, ResultInitializerExpr));
        }
    }
}
