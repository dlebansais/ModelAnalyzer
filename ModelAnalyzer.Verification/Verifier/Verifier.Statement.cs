namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a code verifier.
/// </summary>
internal partial class Verifier : IDisposable
{
    private bool AddStatementListExecution(VerificationContext verificationContext, BlockScope block)
    {
        foreach (Statement Statement in block.StatementList)
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
            case PrivateMethodCallStatement PrivateMethodCall:
                Result = AddPrivateMethodCallExecution(verificationContext, PrivateMethodCall);
                IsAdded = true;
                break;
            case PublicMethodCallStatement PublicMethodCall:
                Result = AddPublicMethodCallExecution(verificationContext, PublicMethodCall);
                IsAdded = true;
                break;
            case ReturnStatement Return:
                Result = AddReturnExecution(verificationContext, Return);
                IsAdded = true;
                break;
            case ForLoopStatement ForLoop:
                Result = AddForLoopExecution(verificationContext, ForLoop);
                IsAdded = true;
                break;
        }

        Debug.Assert(IsAdded);

        return Result;
    }

    private bool AddAssignmentExecution(VerificationContext verificationContext, AssignmentStatement assignmentStatement)
    {
        if (!BuildExpression(verificationContext, assignmentStatement.Expression, out IExprBase<IExprCapsule, IExprCapsule> SourceExpr))
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

        IVariableName VariableBlockName = ObjectManager.CreateBlockName(HostMethod is null ? verificationContext.Instance : null, HostMethod, VariableName!);
        Variable Destination = new(VariableBlockName, VariableType!);

        if (assignmentStatement.DestinationIndex is null)
            verificationContext.ObjectManager.Assign(verificationContext.Branch, Destination, SourceExpr);
        else
        {
            // Index is either an int literal or a single variable. This can't possibly fail.
            bool IndexBuildSuccess = BuildExpression(verificationContext, assignmentStatement.DestinationIndex, out IExprBase<IIntExprCapsule, IIntExprCapsule> IndexExpr);
            Debug.Assert(IndexBuildSuccess);

            Debug.Assert(IndexExpr is IExprSingle<IIntExprCapsule>);
            IIntExprCapsule DestinationIndexExpr = ((IExprSingle<IIntExprCapsule>)IndexExpr).MainExpression;

            Debug.Assert(SourceExpr is IExprSingle<IExprCapsule>);
            IExprCapsule SourceValueExpr = ((IExprSingle<IExprCapsule>)SourceExpr).MainExpression;

            verificationContext.ObjectManager.AssignElement(verificationContext.Branch, Destination, DestinationIndexExpr, SourceValueExpr);
        }

        return true;
    }

    private bool AddConditionalExecution(VerificationContext verificationContext, ConditionalStatement conditionalStatement)
    {
        if (!BuildExpression(verificationContext, conditionalStatement.Condition, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> ConditionExpr))
            return false;

        IBoolExprCapsule TrueBranchExpr;
        IBoolExprCapsule FalseBranchExpr;

        Debug.Assert(ConditionExpr is IExprSingle<IBoolExprCapsule>);

        TrueBranchExpr = Context.CreateTrueBranchExpr(verificationContext.Branch, ConditionExpr.MainExpression);
        FalseBranchExpr = Context.CreateFalseBranchExpr(verificationContext.Branch, ConditionExpr.MainExpression);

        verificationContext.ObjectManager.BeginBranch(out AliasTable BeforeWhenTrue);

        VerificationContext TrueBranchVerificationContext = verificationContext with { Branch = TrueBranchExpr, HostBlock = conditionalStatement.WhenTrueBlock };
        bool TrueBranchResult = AddStatementListExecution(TrueBranchVerificationContext, conditionalStatement.WhenTrueBlock);

        // For the else branch, start alias indexes from what they are at the end of the if branch.
        verificationContext.ObjectManager.EndBranch(BeforeWhenTrue, out List<VariableAlias> AliasesOnlyWhenTrue, out AliasTable WhenTrueAliasTable);

        AliasTable BeforeWhenFalse = WhenTrueAliasTable;
        VerificationContext FalseBranchVerificationContext = verificationContext with { Branch = FalseBranchExpr, HostBlock = conditionalStatement.WhenFalseBlock };
        bool FalseBranchResult = AddStatementListExecution(FalseBranchVerificationContext, conditionalStatement.WhenFalseBlock);

        verificationContext.ObjectManager.EndBranch(BeforeWhenFalse, out List<VariableAlias> AliasesOnlyWhenFalse, out AliasTable WhenFalseAliasTable);

        verificationContext.ObjectManager.MergeBranches(WhenTrueAliasTable, TrueBranchExpr, AliasesOnlyWhenTrue, WhenFalseAliasTable, FalseBranchExpr, AliasesOnlyWhenFalse);

        return TrueBranchResult && FalseBranchResult;
    }

    private bool AddPrivateMethodCallExecution(VerificationContext verificationContext, PrivateMethodCallStatement methodCallStatement)
    {
        bool Result = true;
        bool IsExecuted = false;

        foreach (var Entry in MethodTable)
            if (Entry.Key == methodCallStatement.MethodName)
            {
                Method CalledMethod = Entry.Value;
                Result = AddPrivateMethodCallExecution(verificationContext, methodCallStatement, CalledMethod);
                IsExecuted = true;
                break;
            }

        Debug.Assert(IsExecuted);

        return Result;
    }

    private bool AddPrivateMethodCallExecution(VerificationContext verificationContext, PrivateMethodCallStatement methodCallStatement, Method calledMethod)
    {
        List<Argument> ArgumentList = methodCallStatement.ArgumentList;

        int Index = 0;
        foreach (var Entry in calledMethod.ParameterTable)
        {
            Argument Argument = ArgumentList[Index++];
            Parameter Parameter = Entry.Value;

            if (!BuildExpression(verificationContext, Argument.Expression, out IExprBase<IExprCapsule, IExprCapsule> InitializerExpr))
                return false;

            verificationContext.ObjectManager.CreateVariable(owner: null, calledMethod, Parameter.Name, Parameter.Type, verificationContext.Branch, InitializerExpr);
        }

        VerificationContext CallVerificationContext = verificationContext with { HostMethod = calledMethod, HostBlock = calledMethod.RootBlock, ResultLocal = null };

        if (!AddMethodRequires(CallVerificationContext, methodCallStatement.CallerClassName, methodCallStatement.LocationId, checkOpposite: true))
            return false;

        if (!AddStatementListExecution(CallVerificationContext, calledMethod.RootBlock))
            return false;

        if (!AddMethodEnsures(CallVerificationContext, keepNormal: true))
            return false;

        return true;
    }

    private bool AddPublicMethodCallExecution(VerificationContext verificationContext, PublicMethodCallStatement methodCallStatement)
    {
        bool Result = false;
        ClassModel ClassModel;
        Instance CalledInstance;

        if (methodCallStatement.IsStatic)
        {
            ClassModel = GetClassModel(verificationContext, methodCallStatement.ClassName);
            CalledInstance = new() { ClassModel = ClassModel, Expr = verificationContext.ObjectManager.Context.Null };
        }
        else
        {
            ClassModel = GetLastClassModel(verificationContext, methodCallStatement.VariablePath);

            BuildVariableValueExpression(verificationContext, methodCallStatement.VariablePath, out IExprBase<IExprCapsule, IExprCapsule> CalledClassExpr);

            Debug.Assert(CalledClassExpr.MainExpression is IObjectRefExprCapsule);
            IObjectRefExprCapsule CalledInstanceExpr = (IObjectRefExprCapsule)CalledClassExpr.MainExpression;
            CalledInstance = new() { ClassModel = ClassModel, Expr = CalledInstanceExpr };
        }

        bool IsExecuted = false;

        foreach (var Entry in ClassModel.MethodTable)
            if (Entry.Key == methodCallStatement.MethodName)
            {
                Method CalledMethod = Entry.Value;
                Result = AddPublicMethodCallExecution(verificationContext, methodCallStatement, CalledInstance, CalledMethod);
                IsExecuted = true;
                break;
            }

        Debug.Assert(IsExecuted);

        return Result;
    }

    private bool AddPublicMethodCallExecution(VerificationContext verificationContext, PublicMethodCallStatement methodCallStatement, Instance calledInstance, Method calledMethod)
    {
        List<Argument> ArgumentList = methodCallStatement.ArgumentList;

        int Index = 0;
        foreach (var Entry in calledMethod.ParameterTable)
        {
            Argument Argument = ArgumentList[Index++];
            Parameter Parameter = Entry.Value;

            if (!BuildExpression(verificationContext, Argument.Expression, out IExprBase<IExprCapsule, IExprCapsule> InitializerExpr))
                return false;

            verificationContext.ObjectManager.CreateVariable(owner: null, calledMethod, Parameter.Name, Parameter.Type, verificationContext.Branch, InitializerExpr);
        }

        VerificationContext CallVerificationContext = verificationContext with
        {
            Instance = calledInstance,
            HostMethod = calledMethod,
            HostBlock = calledMethod.RootBlock,
            ResultLocal = null,
        };

        bool IsStaticCall = calledInstance.Expr == verificationContext.ObjectManager.Context.Null;

        if (!AddMethodRequires(CallVerificationContext, methodCallStatement.CallerClassName, methodCallStatement.LocationId, checkOpposite: true))
            return false;

        if (!IsStaticCall)
            verificationContext.ObjectManager.ClearState(calledInstance);

        if (!AddMethodEnsures(CallVerificationContext, keepNormal: true))
            return false;

        if (!IsStaticCall)
            if (!AddClassInvariant(CallVerificationContext))
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

            if (!BuildExpression(verificationContext, ReturnExpression, out IExprBase<IExprCapsule, IExprCapsule> ResultInitializerExpr))
                return false;

            IVariableName ResultLocalBlockName = ObjectManager.CreateBlockName(owner: null, HostMethod, ResultLocal.Name);
            Variable ResultLocalVariable = new(ResultLocalBlockName, ResultLocal.Type);

            verificationContext.ObjectManager.Assign(verificationContext.Branch, ResultLocalVariable, ResultInitializerExpr);
        }

        return true;
    }

    private bool AddForLoopExecution(VerificationContext verificationContext, ForLoopStatement forLoopStatement)
    {
        // TODO
        return true;
    }
}
