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
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;
        Expression Source = assignmentStatement.Expression;
        string DestinationName = assignmentStatement.DestinationName.Text;
        bool Result = false;

        foreach (KeyValuePair<PropertyName, Property> Entry in verificationContext.PropertyTable)
            if (Entry.Key.Text == DestinationName)
            {
                Property Property = Entry.Value;
                IVariableName PropertyBlockName = ObjectManager.CreateBlockName(ObjectManager.ThisObject, hostMethod: null, Property.Name);
                Variable Destination = new(PropertyBlockName, Property.Type);
                Result = AddAssignmentExecution(verificationContext, Destination, Source);
                break;
            }

        foreach (KeyValuePair<FieldName, Field> Entry in verificationContext.FieldTable)
            if (Entry.Key.Text == DestinationName)
            {
                Field Field = Entry.Value;
                IVariableName FieldBlockName = ObjectManager.CreateBlockName(ObjectManager.ThisObject, hostMethod: null, Field.Name);
                Variable Destination = new(FieldBlockName, Field.Type);
                Result = AddAssignmentExecution(verificationContext, Destination, Source);
                break;
            }

        foreach (KeyValuePair<LocalName, Local> Entry in HostMethod.LocalTable)
            if (Entry.Key.Text == DestinationName)
            {
                Local Local = Entry.Value;
                IVariableName LocalBlockName = ObjectManager.CreateBlockName(owner: null, HostMethod, Local.Name);
                Variable Destination = new(LocalBlockName, Local.Type);
                Result = AddAssignmentExecution(verificationContext, Destination, Source);
                break;
            }

        return Result;
    }

    private bool AddAssignmentExecution(VerificationContext verificationContext, Variable destination, Expression source)
    {
        if (!BuildExpression(verificationContext, source, out Expr SourceExpr))
            return false;

        verificationContext.ObjectManager.Assign(verificationContext.Branch, destination, SourceExpr);

        return true;
    }

    private bool AddConditionalExecution(VerificationContext verificationContext, ConditionalStatement conditionalStatement)
    {
        if (!BuildExpression(verificationContext, conditionalStatement.Condition, out BoolExpr ConditionExpr))
            return false;

        BoolExpr TrueBranchExpr;
        BoolExpr FalseBranchExpr;

        TrueBranchExpr = Context.CreateTrueBranchExpr(verificationContext.Branch, ConditionExpr);
        FalseBranchExpr = Context.CreateFalseBranchExpr(verificationContext.Branch, ConditionExpr);

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

            if (!BuildExpression(verificationContext, Argument.Expression, out Expr InitializerExpr))
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

            if (!BuildExpression(verificationContext, ReturnExpression, out Expr ResultInitializerExpr))
                return false;

            IVariableName ResultLocalBlockName = ObjectManager.CreateBlockName(owner: null, HostMethod, ResultLocal.Name);
            Variable ResultLocalVariable = new(ResultLocalBlockName, ResultLocal.Type);

            verificationContext.ObjectManager.Assign(verificationContext.Branch, ResultLocalVariable, ResultInitializerExpr);
        }

        return true;
    }
}
