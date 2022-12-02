namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using Microsoft.Z3;

public partial class Verifier : IDisposable
{
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
}
