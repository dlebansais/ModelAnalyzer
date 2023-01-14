﻿namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a code verifier.
/// </summary>
internal partial class Verifier : IDisposable
{
    private bool AddStatementListExecution(VerificationContext verificationContext, List<Statement> statementList)
    {
        foreach (Statement Statement in statementList)
            if (!AddStatementExecution(verificationContext, Statement))
                return false;

        return true;
    }

    private bool AddStatementExecution(VerificationContext verificationContext, Statement statement)
    {
        bool Result = false;
        bool IsAdded = false;

        switch (statement)
        {
            case AssignmentStatement Assignment:
                Result = AddAssignmentExecution(verificationContext, Assignment);
                IsAdded = true;
                break;
            case ConditionalStatement Conditional:
                Result = AddConditionalExecution(verificationContext, Conditional);
                IsAdded = true;
                break;
            case MethodCallStatement MethodCall:
                Result = AddMethodCallExecution(verificationContext, MethodCall);
                IsAdded = true;
                break;
            case ReturnStatement Return:
                Result = AddReturnExecution(verificationContext, Return);
                IsAdded = true;
                break;
        }

        Debug.Assert(IsAdded);

        return Result;
    }

    private bool AddAssignmentExecution(VerificationContext verificationContext, AssignmentStatement assignmentStatement)
    {
        if (!BuildExpression(verificationContext, assignmentStatement.Expression, out IExprSet<IExprCapsule> SourceExpr))
            return false;

        string Name = assignmentStatement.DestinationName.Text;
        Method? HostMethod = null;
        IVariableName? VariableName = null;
        ExpressionType? VariableType = null;

        LookupProperty(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);
        LookupField(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);
        LookupLocal(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);

        Debug.Assert(VariableName is not null);
        Debug.Assert(VariableType is not null);

        IVariableName VariableBlockName = ObjectManager.CreateBlockName(HostMethod is null ? ObjectManager.ThisObject : null, HostMethod, VariableName!);
        Variable Destination = new(VariableBlockName, VariableType!);

        verificationContext.ObjectManager.Assign(verificationContext.Branch, Destination, SourceExpr);

        return true;
    }

    private bool AddConditionalExecution(VerificationContext verificationContext, ConditionalStatement conditionalStatement)
    {
        if (!BuildExpression(verificationContext, conditionalStatement.Condition, out IExprSet<IBoolExprCapsule> ConditionExpr))
            return false;

        IBoolExprCapsule TrueBranchExpr;
        IBoolExprCapsule FalseBranchExpr;

        Debug.Assert(ConditionExpr.IsSingle);

        TrueBranchExpr = Context.CreateTrueBranchExpr(verificationContext.Branch, ConditionExpr.MainExpression);
        FalseBranchExpr = Context.CreateFalseBranchExpr(verificationContext.Branch, ConditionExpr.MainExpression);

        verificationContext.ObjectManager.BeginBranch(out AliasTable BeforeWhenTrue);

        VerificationContext TrueBranchVerificationContext = verificationContext with { Branch = TrueBranchExpr };
        bool TrueBranchResult = AddStatementListExecution(TrueBranchVerificationContext, conditionalStatement.WhenTrueStatementList);

        // For the else branch, start alias indexes from what they are at the end of the if branch.
        verificationContext.ObjectManager.EndBranch(BeforeWhenTrue, out List<VariableAlias> AliasesOnlyWhenTrue, out AliasTable WhenTrueAliasTable);

        AliasTable BeforeWhenFalse = WhenTrueAliasTable;
        VerificationContext FalseBranchVerificationContext = verificationContext with { Branch = FalseBranchExpr };
        bool FalseBranchResult = AddStatementListExecution(FalseBranchVerificationContext, conditionalStatement.WhenFalseStatementList);

        verificationContext.ObjectManager.EndBranch(BeforeWhenFalse, out List<VariableAlias> AliasesOnlyWhenFalse, out AliasTable WhenFalseAliasTable);

        verificationContext.ObjectManager.MergeBranches(WhenTrueAliasTable, TrueBranchExpr, AliasesOnlyWhenTrue, WhenFalseAliasTable, FalseBranchExpr, AliasesOnlyWhenFalse);

        return TrueBranchResult && FalseBranchResult;
    }

    private bool AddMethodCallExecution(VerificationContext verificationContext, MethodCallStatement methodCallStatement)
    {
        bool Result = true;
        bool IsExecuted = false;

        foreach (var Entry in MethodTable)
            if (Entry.Key == methodCallStatement.MethodName)
            {
                Method CalledMethod = Entry.Value;
                Result = AddMethodCallExecution(verificationContext, methodCallStatement, CalledMethod);
                IsExecuted = true;
                break;
            }

        Debug.Assert(IsExecuted);

        return Result;
    }

    private bool AddMethodCallExecution(VerificationContext verificationContext, MethodCallStatement methodCallStatement, Method calledMethod)
    {
        List<Argument> ArgumentList = methodCallStatement.ArgumentList;

        int Index = 0;
        foreach (var Entry in calledMethod.ParameterTable)
        {
            Argument Argument = ArgumentList[Index++];
            Parameter Parameter = Entry.Value;

            if (!BuildExpression(verificationContext, Argument.Expression, out IExprSet<IExprCapsule> InitializerExpr))
                return false;

            verificationContext.ObjectManager.CreateVariable(owner: null, calledMethod, Parameter.Name, Parameter.Type, verificationContext.Branch, InitializerExpr);
        }

        VerificationContext CallVerificationContext = verificationContext with { HostMethod = calledMethod, ResultLocal = null };

        if (!AddMethodRequires(CallVerificationContext, checkOpposite: true))
            return false;

        if (!AddStatementListExecution(CallVerificationContext, calledMethod.StatementList))
            return false;

        if (!AddMethodEnsures(CallVerificationContext, keepNormal: true))
            return false;

        return true;
    }

    private bool AddReturnExecution(VerificationContext verificationContext, ReturnStatement returnStatement)
    {
        Expression? ReturnExpression = (Expression?)returnStatement.Expression;

        if (ReturnExpression is not null)
        {
            Debug.Assert(verificationContext.HostMethod is not null);
            Debug.Assert(verificationContext.ResultLocal is not null);

            Method HostMethod = verificationContext.HostMethod!;
            Local ResultLocal = verificationContext.ResultLocal!;

            if (!BuildExpression(verificationContext, ReturnExpression, out IExprSet<IExprCapsule> ResultInitializerExpr))
                return false;

            IVariableName ResultLocalBlockName = ObjectManager.CreateBlockName(owner: null, HostMethod, ResultLocal.Name);
            Variable ResultLocalVariable = new(ResultLocalBlockName, ResultLocal.Type);

            verificationContext.ObjectManager.Assign(verificationContext.Branch, ResultLocalVariable, ResultInitializerExpr);
        }

        return true;
    }
}