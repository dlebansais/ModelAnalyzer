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
        bool Result = true;
        bool IsAdded = false;

        switch (statement)
        {
            case AssignmentStatement Assignment:
                AddAssignmentExecution(verificationContext, Assignment);
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
                AddReturnExecution(verificationContext, Return);
                IsAdded = true;
                break;
        }

        Debug.Assert(IsAdded);

        return Result;
    }

    private bool AddAssignmentExecution(VerificationContext verificationContext, AssignmentStatement assignmentStatement)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        ReadOnlyFieldTable FieldTable = verificationContext.FieldTable;
        Method HostMethod = verificationContext.HostMethod!;
        Expression Source = assignmentStatement.Expression;
        string DestinationName = assignmentStatement.DestinationName.Text;
        bool Result = false;

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
            if (Entry.Key.Text == DestinationName)
            {
                Field Field = Entry.Value;
                Variable Destination = new Variable(Field.Name, Field.Type);
                Result = AddAssignmentExecution(verificationContext, Destination, Source);
                break;
            }

        foreach (KeyValuePair<LocalName, Local> Entry in HostMethod.LocalTable)
            if (Entry.Key.Text == DestinationName)
            {
                Local Local = Entry.Value;
                LocalName LocalBlockName = CreateLocalBlockName(HostMethod, Local);
                Variable Destination = new(LocalBlockName, Local.Type);
                Result = AddAssignmentExecution(verificationContext, Destination, Source);
                break;
            }

        return Result;
    }

    private bool AddAssignmentExecution(VerificationContext verificationContext, Variable destination, Expression source)
    {
        AliasTable AliasTable = verificationContext.AliasTable;
        if (!BuildExpression(verificationContext, source, out Expr SourceExpr))
            return false;

        AliasTable.IncrementAlias(destination);
        VariableAlias DestinationNameAlias = AliasTable.GetAlias(destination);
        Expr DestinationExpr = CreateVariableExpr(DestinationNameAlias.ToString(), source.GetExpressionType(verificationContext));

        AddToSolver(verificationContext, Context.MkEq(DestinationExpr, SourceExpr));
        return true;
    }

    private bool AddConditionalExecution(VerificationContext verificationContext, ConditionalStatement conditionalStatement)
    {
        AliasTable AliasTable = verificationContext.AliasTable;

        if (!BuildExpression(verificationContext, conditionalStatement.Condition, out BoolExpr ConditionExpr))
            return false;

        BoolExpr TrueBranchExpr;
        BoolExpr FalseBranchExpr;

        if (verificationContext.Branch is BoolExpr Branch)
        {
            TrueBranchExpr = Context.MkAnd(Branch, ConditionExpr);
            FalseBranchExpr = Context.MkAnd(Branch, Context.MkNot(ConditionExpr));
        }
        else
        {
            TrueBranchExpr = ConditionExpr;
            FalseBranchExpr = Context.MkNot(ConditionExpr);
        }

        AliasTable BeforeWhenTrue = AliasTable.Clone();
        VerificationContext TrueBranchVerificationContext = verificationContext with { Branch = TrueBranchExpr };
        bool TrueBranchResult = AddStatementListExecution(TrueBranchVerificationContext, conditionalStatement.WhenTrueStatementList);
        List<VariableAlias> AliasesOnlyWhenTrue = AliasTable.GetAliasDifference(BeforeWhenTrue);

        // For the else branch, start alias indexes from what they are at the end of the if branch.
        AliasTable WhenTrueAliasTable = AliasTable.Clone();

        AliasTable BeforeWhenFalse = WhenTrueAliasTable;
        VerificationContext FalseBranchVerificationContext = verificationContext with { Branch = FalseBranchExpr };
        bool FalseBranchResult = AddStatementListExecution(FalseBranchVerificationContext, conditionalStatement.WhenFalseStatementList);
        List<VariableAlias> AliasesOnlyWhenFalse = AliasTable.GetAliasDifference(BeforeWhenFalse);

        AliasTable WhenFalseAliasTable = AliasTable.Clone();

        // Merge aliases from the if branch (the table currently contains the end of the end branch).
        AliasTable.Merge(WhenTrueAliasTable, out List<Variable> UpdatedNameList);

        AddConditionalAliases(TrueBranchVerificationContext, AliasesOnlyWhenFalse);
        AddConditionalAliases(FalseBranchVerificationContext, AliasesOnlyWhenTrue);

        foreach (Variable Variable in UpdatedNameList)
        {
            ExpressionType VariableType = Variable.Type;

            VariableAlias NameAlias = AliasTable.GetAlias(Variable);
            Expr DestinationExpr = CreateVariableExpr(NameAlias.ToString(), VariableType);

            VariableAlias WhenTrueNameAlias = WhenTrueAliasTable.GetAlias(Variable);
            Expr WhenTrueSourceExpr = CreateVariableExpr(WhenTrueNameAlias.ToString(), VariableType);
            BoolExpr WhenTrueInitExpr = Context.MkEq(DestinationExpr, WhenTrueSourceExpr);

            VariableAlias WhenFalseNameAlias = WhenFalseAliasTable.GetAlias(Variable);
            Expr WhenFalseSourceExpr = CreateVariableExpr(WhenFalseNameAlias.ToString(), VariableType);
            BoolExpr WhenFalseInitExpr = Context.MkEq(DestinationExpr, WhenFalseSourceExpr);

            AddToSolver(TrueBranchVerificationContext, WhenTrueInitExpr);
            AddToSolver(FalseBranchVerificationContext, WhenFalseInitExpr);
        }

        return TrueBranchResult && FalseBranchResult;
    }

    private void AddConditionalAliases(VerificationContext verificationContext, List<VariableAlias> aliasList)
    {
        foreach (VariableAlias Alias in aliasList)
        {
            Variable Variable = Alias.Variable;
            ExpressionType VariableType = Variable.Type;

            Expr FieldExpr = CreateVariableExpr(Alias.ToString(), VariableType);
            Expr InitializerExpr = GetDefaultExpr(VariableType);
            BoolExpr InitExpr = Context.MkEq(FieldExpr, InitializerExpr);

            AddToSolver(verificationContext, InitExpr);
        }
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
        AliasTable AliasTable = verificationContext.AliasTable;
        List<Argument> ArgumentList = methodCallStatement.ArgumentList;

        int Index = 0;
        foreach (var Entry in calledMethod.ParameterTable)
        {
            Argument Argument = ArgumentList[Index++];
            Parameter Parameter = Entry.Value;
            ParameterName ParameterBlockName = CreateParameterBlockName(calledMethod, Parameter);
            Variable ParameterVariable = new(ParameterBlockName, Parameter.Type);

            AliasTable.AddOrIncrement(ParameterVariable);
            VariableAlias FieldNameAlias = AliasTable.GetAlias(ParameterVariable);

            Expr TemporaryLocalExpr = CreateVariableExpr(FieldNameAlias.ToString(), Parameter.Type);

            if (!BuildExpression(verificationContext, Argument.Expression, out Expr InitializerExpr))
                return false;

            BoolExpr InitExpr = Context.MkEq(TemporaryLocalExpr, InitializerExpr);

            AddToSolver(verificationContext, InitExpr);
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
            AliasTable AliasTable = verificationContext.AliasTable;

            if (!BuildExpression(verificationContext, ReturnExpression, out Expr ResultInitializerExpr))
                return false;

            LocalName ResultLocalBlockName = CreateLocalBlockName(HostMethod, ResultLocal);
            Variable ResultLocalVariable = new(ResultLocalBlockName, ResultLocal.Type);

            AliasTable.IncrementAlias(ResultLocalVariable);
            VariableAlias ResultLocalAlias = AliasTable.GetAlias(ResultLocalVariable);
            Expr ResultLocalExpr = CreateVariableExpr(ResultLocalAlias.ToString(), ResultLocal.Type);

            AddToSolver(verificationContext, Context.MkEq(ResultLocalExpr, ResultInitializerExpr));
        }

        return true;
    }
}
