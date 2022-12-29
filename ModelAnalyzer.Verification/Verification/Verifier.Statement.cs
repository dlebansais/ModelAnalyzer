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
    private void AddStatementListExecution(Solver solver, AliasTable aliasTable, ReadOnlyParameterTable parameterTable, ref Field? resultField, BoolExpr branch, List<Statement> statementList)
    {
        foreach (Statement Statement in statementList)
            AddStatementExecution(solver, aliasTable, parameterTable, ref resultField, branch, Statement);
    }

    private void AddStatementExecution(Solver solver, AliasTable aliasTable, ReadOnlyParameterTable parameterTable, ref Field? resultField, BoolExpr branch, Statement statement)
    {
        bool IsAdded = false;

        switch (statement)
        {
            case AssignmentStatement Assignment:
                AddAssignmentExecution(solver, aliasTable, parameterTable, ref resultField, branch, Assignment);
                IsAdded = true;
                break;
            case ConditionalStatement Conditional:
                AddConditionalExecution(solver, aliasTable, parameterTable, ref resultField, branch, Conditional);
                IsAdded = true;
                break;
            case ReturnStatement Return:
                AddReturnExecution(solver, aliasTable, parameterTable, ref resultField, branch, Return);
                IsAdded = true;
                break;
        }

        Debug.Assert(IsAdded);
    }

    private void AddAssignmentExecution(Solver solver, AliasTable aliasTable, ReadOnlyParameterTable parameterTable, ref Field? resultField, BoolExpr branch, AssignmentStatement assignmentStatement)
    {
        Expr SourceExpr = BuildExpression<Expr>(aliasTable, parameterTable, resultField, assignmentStatement.Expression);
        IVariable Destination = ClassModel.GetVariable(FieldTable, parameterTable, resultField, assignmentStatement.DestinationName);

        aliasTable.IncrementAlias(Destination);
        VariableAlias DestinationNameAlias = aliasTable.GetAlias(Destination);
        Expr DestinationExpr = CreateVariableExpr(DestinationNameAlias.ToString(), assignmentStatement.Expression.GetExpressionType(FieldTable, parameterTable, resultField));

        AddToSolver(solver, branch, Context.MkEq(DestinationExpr, SourceExpr));
    }

    private void AddConditionalExecution(Solver solver, AliasTable aliasTable, ReadOnlyParameterTable parameterTable, ref Field? resultField, BoolExpr branch, ConditionalStatement conditionalStatement)
    {
        BoolExpr ConditionExpr = BuildExpression<BoolExpr>(aliasTable, parameterTable, resultField, conditionalStatement.Condition);
        BoolExpr TrueBranchExpr = Context.MkAnd(branch, ConditionExpr);
        BoolExpr FalseBranchExpr = Context.MkAnd(branch, Context.MkNot(ConditionExpr));

        AliasTable BeforeWhenTrue = aliasTable.Clone();
        AddStatementListExecution(solver, aliasTable, parameterTable, ref resultField, TrueBranchExpr, conditionalStatement.WhenTrueStatementList);
        List<VariableAlias> AliasesOnlyWhenTrue = aliasTable.GetAliasDifference(BeforeWhenTrue);

        // For the else branch, start alias indexes from what they are at the end of the if branch.
        AliasTable WhenTrueAliasTable = aliasTable.Clone();

        AliasTable BeforeWhenFalse = WhenTrueAliasTable;
        AddStatementListExecution(solver, aliasTable, parameterTable, ref resultField, FalseBranchExpr, conditionalStatement.WhenFalseStatementList);
        List<VariableAlias> AliasesOnlyWhenFalse = aliasTable.GetAliasDifference(BeforeWhenFalse);

        AliasTable WhenFalseAliasTable = aliasTable.Clone();

        // Merge aliases from the if branch (the table currently contains the end of the end branch).
        aliasTable.Merge(WhenTrueAliasTable, out List<IVariable> UpdatedNameList);

        AddConditionalAliases(solver, TrueBranchExpr, AliasesOnlyWhenFalse);
        AddConditionalAliases(solver, FalseBranchExpr, AliasesOnlyWhenTrue);

        foreach (IVariable Variable in UpdatedNameList)
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
    }

    private void AddConditionalAliases(Solver solver, BoolExpr branchExpr, List<VariableAlias> aliasList)
    {
        foreach (VariableAlias Alias in aliasList)
        {
            IVariable Variable = Alias.Variable;
            ExpressionType FieldType = Variable.Type;

            Expr FieldExpr = CreateVariableExpr(Alias.ToString(), FieldType);
            Expr InitializerExpr = CreateVariableInitializer(Variable);
            BoolExpr InitExpr = Context.MkEq(FieldExpr, InitializerExpr);

            AddToSolver(solver, branchExpr, InitExpr);
        }
    }

    private void AddReturnExecution(Solver solver, AliasTable aliasTable, ReadOnlyParameterTable parameterTable, ref Field? resultField, BoolExpr branch, ReturnStatement returnStatement)
    {
        Expression? ReturnExpression = (Expression?)returnStatement.Expression;

        if (ReturnExpression is null)
            resultField = null;
        else
        {
            ExpressionType ResultType = ReturnExpression.GetExpressionType(FieldTable, parameterTable, resultField: null);
            resultField = new Field() { Name = new FieldName() { Text = Ensure.ResultKeyword }, Type = ResultType, Initializer = null };

            Expr ResultFieldExpr = CreateVariableExpr(Ensure.ResultKeyword, ResultType);
            Expr ResultInitializerExpr = BuildExpression<Expr>(aliasTable, parameterTable, resultField: null, ReturnExpression);

            AddToSolver(solver, branch, Context.MkEq(ResultFieldExpr, ResultInitializerExpr));
        }
    }
}
