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
    private void AddStatementListExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, List<IStatement> statementList)
    {
        foreach (IStatement Statement in statementList)
            AddStatementExecution(solver, aliasTable, branch, Statement);
    }

    private void AddStatementExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, IStatement statement)
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
        string DestinationNameAlias = aliasTable.GetAlias(DestinationName);
        IntExpr DestinationExpr = Context.MkIntConst(DestinationNameAlias);

        AddToSolver(solver, branch, Context.MkEq(DestinationExpr, SourceExpr));
    }

    private void AddConditionalExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, ConditionalStatement conditionalStatement)
    {
        BoolExpr ConditionExpr = BuildExpression<BoolExpr>(aliasTable, conditionalStatement.Condition);
        BoolExpr TrueBranchExpr = Context.MkAnd(branch, ConditionExpr);
        BoolExpr FalseBranchExpr = Context.MkAnd(branch, Context.MkNot(ConditionExpr));

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
            IntExpr AliasExpr = Context.MkIntConst(NameAlias);
            BoolExpr InitExpr = Context.MkEq(AliasExpr, Zero);
            AddToSolver(solver, TrueBranchExpr, InitExpr);
        }

        foreach (string NameAlias in AliasesOnlyWhenTrue)
        {
            IntExpr AliasExpr = Context.MkIntConst(NameAlias);
            BoolExpr InitExpr = Context.MkEq(AliasExpr, Zero);
            AddToSolver(solver, FalseBranchExpr, InitExpr);
        }

        foreach (string Name in UpdatedNameList)
        {
            string NameAlias = aliasTable.GetAlias(Name);
            IntExpr DestinationExpr = Context.MkIntConst(NameAlias);

            string WhenTrueNameAlias = WhenTrueAliasTable.GetAlias(Name);
            IntExpr WhenTrueSourceExpr = Context.MkIntConst(WhenTrueNameAlias);
            BoolExpr WhenTrueInitExpr = Context.MkEq(DestinationExpr, WhenTrueSourceExpr);

            string WhenFalseNameAlias = WhenFalseAliasTable.GetAlias(Name);
            IntExpr WhenFalseSourceExpr = Context.MkIntConst(WhenFalseNameAlias);
            BoolExpr WhenFalseInitExpr = Context.MkEq(DestinationExpr, WhenFalseSourceExpr);

            AddToSolver(solver, TrueBranchExpr, WhenTrueInitExpr);
            AddToSolver(solver, FalseBranchExpr, WhenFalseInitExpr);
        }
    }

    private void AddReturnExecution(Solver solver, AliasTable aliasTable, BoolExpr branch, ReturnStatement returnStatement)
    {
    }
}
