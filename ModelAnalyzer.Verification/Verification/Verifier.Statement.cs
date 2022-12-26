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
    private void AddStatementListExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, List<Statement> statementList)
    {
        foreach (Statement Statement in statementList)
            AddStatementExecution(solver, aliasTable, branch, Statement);
    }

    private void AddStatementExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, Statement statement)
    {
        bool IsAdded = false;

        switch (statement)
        {
            case AssignmentStatement Assignment:
                AddAssignmentExecution(solver, aliasTable, branch, Assignment);
                IsAdded = true;
                break;
            case ConditionalStatement Conditional:
                AddConditionalExecution(solver, aliasTable, branch, Conditional);
                IsAdded = true;
                break;
            case ReturnStatement Return:
                AddReturnExecution(solver, aliasTable, branch, Return);
                IsAdded = true;
                break;
        }

        Debug.Assert(IsAdded);
    }

    private void AddAssignmentExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, AssignmentStatement assignmentStatement)
    {
        Expr SourceExpr = BuildExpression<Expr>(aliasTable, assignmentStatement.Expression);

        string DestinationName = assignmentStatement.Destination.Name;
        aliasTable.IncrementNameAlias(DestinationName);
        AliasName DestinationNameAlias = aliasTable.GetAlias(DestinationName);
        Expr DestinationExpr = CreateVariableExpr(DestinationNameAlias, assignmentStatement.Expression.ExpressionType);

        AddToSolver(solver, branch, Context.MkEq(DestinationExpr, SourceExpr));
    }

    private void AddConditionalExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, ConditionalStatement conditionalStatement)
    {
        BoolExpr ConditionExpr = BuildExpression<BoolExpr>(aliasTable, conditionalStatement.Condition);
        BoolExpr TrueBranchExpr = Context.MkAnd(branch, ConditionExpr);
        BoolExpr FalseBranchExpr = Context.MkAnd(branch, Context.MkNot(ConditionExpr));

        AliasTable BeforeWhenTrue = aliasTable.Clone();
        AddStatementListExecution(solver, aliasTable, TrueBranchExpr, conditionalStatement.WhenTrueStatementList);
        List<AliasName> AliasesOnlyWhenTrue = aliasTable.GetAliasDifference(BeforeWhenTrue);

        // For the else branch, start alias indexes from what they are at the end of the if branch.
        AliasTable WhenTrueAliasTable = aliasTable.Clone();

        AliasTable BeforeWhenFalse = WhenTrueAliasTable;
        AddStatementListExecution(solver, aliasTable, FalseBranchExpr, conditionalStatement.WhenFalseStatementList);
        List<AliasName> AliasesOnlyWhenFalse = aliasTable.GetAliasDifference(BeforeWhenFalse);

        AliasTable WhenFalseAliasTable = aliasTable.Clone();

        // Merge aliases from the if branche (the table currently contains the end of the end branch).
        aliasTable.Merge(WhenTrueAliasTable, out List<string> UpdatedNameList);

        AddConditionalAliases(solver, TrueBranchExpr, AliasesOnlyWhenFalse);
        AddConditionalAliases(solver, FalseBranchExpr, AliasesOnlyWhenTrue);

        foreach (string Name in UpdatedNameList)
        {
            FieldName FieldName = new() { Name = Name };
            foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
                if (Entry.Key == FieldName)
                {
                    Field Field = Entry.Value;
                    ExpressionType FieldType = Field.VariableType;

                    AliasName NameAlias = aliasTable.GetAlias(Name);
                    Expr DestinationExpr = CreateVariableExpr(NameAlias, FieldType);

                    AliasName WhenTrueNameAlias = WhenTrueAliasTable.GetAlias(Name);
                    Expr WhenTrueSourceExpr = CreateVariableExpr(WhenTrueNameAlias, FieldType);
                    BoolExpr WhenTrueInitExpr = Context.MkEq(DestinationExpr, WhenTrueSourceExpr);

                    AliasName WhenFalseNameAlias = WhenFalseAliasTable.GetAlias(Name);
                    Expr WhenFalseSourceExpr = CreateVariableExpr(WhenFalseNameAlias, FieldType);
                    BoolExpr WhenFalseInitExpr = Context.MkEq(DestinationExpr, WhenFalseSourceExpr);

                    AddToSolver(solver, TrueBranchExpr, WhenTrueInitExpr);
                    AddToSolver(solver, FalseBranchExpr, WhenFalseInitExpr);
                    break;
                }
        }
    }

    private void AddConditionalAliases(Solver solver, BoolExpr branchExpr, List<AliasName> aliasList)
    {
        foreach (AliasName FieldNameAlias in aliasList)
        {
            FieldName FieldName = new() { Name = FieldNameAlias.VariableName };
            foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
                if (Entry.Key == FieldName)
                {
                    Field Field = Entry.Value;
                    ExpressionType FieldType = Field.VariableType;

                    Expr FieldExpr = CreateVariableExpr(FieldNameAlias, FieldType);
                    Expr InitializerExpr = GetFieldInitializer(Field);
                    BoolExpr InitExpr = Context.MkEq(FieldExpr, InitializerExpr);

                    AddToSolver(solver, branchExpr, InitExpr);
                }
        }
    }

    private void AddReturnExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, ReturnStatement returnStatement)
    {
    }
}
