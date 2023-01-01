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
    private bool AddStatementListExecution(VerificationContext verificationContext, Solver solver, BoolExpr branch, List<Statement> statementList)
    {
        foreach (Statement Statement in statementList)
            if (!AddStatementExecution(verificationContext, solver, branch, Statement))
                return false;

        return true;
    }

    private bool AddStatementExecution(VerificationContext verificationContext, Solver solver, BoolExpr branch, Statement statement)
    {
        bool Result = true;
        bool IsAdded = false;

        switch (statement)
        {
            case AssignmentStatement Assignment:
                AddAssignmentExecution(verificationContext, solver, branch, Assignment);
                IsAdded = true;
                break;
            case ConditionalStatement Conditional:
                Result = AddConditionalExecution(verificationContext, solver, branch, Conditional);
                IsAdded = true;
                break;
            case MethodCallStatement MethodCall:
                Result = AddMethodCallExecution(verificationContext, solver, branch, MethodCall);
                IsAdded = true;
                break;
            case ReturnStatement Return:
                AddReturnExecution(verificationContext, solver, branch, Return);
                IsAdded = true;
                break;
        }

        Debug.Assert(IsAdded);

        return Result;
    }

    private void AddAssignmentExecution(VerificationContext verificationContext, Solver solver, BoolExpr branch, AssignmentStatement assignmentStatement)
    {
        Debug.Assert(verificationContext.HostMethod is not null);

        Method HostMethod = verificationContext.HostMethod!;
        Expression Source = assignmentStatement.Expression;
        string DestinationName = assignmentStatement.DestinationName.Text;

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
            if (Entry.Key.Text == DestinationName)
            {
                Field Field = Entry.Value;
                Variable Destination = new Variable(Field.Name, Field.Type);
                AddAssignmentExecution(verificationContext, solver, branch, Destination, Source);
                break;
            }

        foreach (KeyValuePair<LocalName, Local> Entry in HostMethod.LocalTable)
            if (Entry.Key.Text == DestinationName)
            {
                Local Local = Entry.Value;
                LocalName LocalBlockName = CreateLocalBlockName(HostMethod, Local);
                Variable Destination = new(LocalBlockName, Local.Type);
                AddAssignmentExecution(verificationContext, solver, branch, Destination, Source);
                break;
            }
    }

    private void AddAssignmentExecution(VerificationContext verificationContext, Solver solver, BoolExpr branch, Variable destination, Expression source)
    {
        AliasTable AliasTable = verificationContext.AliasTable;
        Expr SourceExpr = BuildExpression<Expr>(verificationContext, source);

        AliasTable.IncrementAlias(destination);
        VariableAlias DestinationNameAlias = AliasTable.GetAlias(destination);
        Expr DestinationExpr = CreateVariableExpr(DestinationNameAlias.ToString(), source.GetExpressionType(verificationContext));

        AddToSolver(solver, branch, Context.MkEq(DestinationExpr, SourceExpr));
    }

    private bool AddConditionalExecution(VerificationContext verificationContext, Solver solver, BoolExpr branch, ConditionalStatement conditionalStatement)
    {
        AliasTable AliasTable = verificationContext.AliasTable;

        BoolExpr ConditionExpr = BuildExpression<BoolExpr>(verificationContext, conditionalStatement.Condition);
        BoolExpr TrueBranchExpr = Context.MkAnd(branch, ConditionExpr);
        BoolExpr FalseBranchExpr = Context.MkAnd(branch, Context.MkNot(ConditionExpr));

        AliasTable BeforeWhenTrue = AliasTable.Clone();
        bool TrueBranchResult = AddStatementListExecution(verificationContext, solver, TrueBranchExpr, conditionalStatement.WhenTrueStatementList);
        List<VariableAlias> AliasesOnlyWhenTrue = AliasTable.GetAliasDifference(BeforeWhenTrue);

        // For the else branch, start alias indexes from what they are at the end of the if branch.
        AliasTable WhenTrueAliasTable = AliasTable.Clone();

        AliasTable BeforeWhenFalse = WhenTrueAliasTable;
        bool FalseBranchResult = AddStatementListExecution(verificationContext, solver, FalseBranchExpr, conditionalStatement.WhenFalseStatementList);
        List<VariableAlias> AliasesOnlyWhenFalse = AliasTable.GetAliasDifference(BeforeWhenFalse);

        AliasTable WhenFalseAliasTable = AliasTable.Clone();

        // Merge aliases from the if branch (the table currently contains the end of the end branch).
        AliasTable.Merge(WhenTrueAliasTable, out List<Variable> UpdatedNameList);

        AddConditionalAliases(solver, TrueBranchExpr, AliasesOnlyWhenFalse);
        AddConditionalAliases(solver, FalseBranchExpr, AliasesOnlyWhenTrue);

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

    private bool AddMethodCallExecution(VerificationContext verificationContext, Solver solver, BoolExpr branch, MethodCallStatement methodCallStatement)
    {
        bool Result = true;
        bool IsExecuted = false;

        foreach (var Entry in MethodTable)
            if (Entry.Key == methodCallStatement.MethodName)
            {
                Method CalledMethod = Entry.Value;
                Result = AddMethodCallExecution(verificationContext, solver, branch, methodCallStatement, CalledMethod);
                IsExecuted = true;
                break;
            }

        Debug.Assert(IsExecuted);

        return Result;
    }

    private bool AddMethodCallExecution(VerificationContext verificationContext, Solver solver, BoolExpr branch, MethodCallStatement methodCallStatement, Method calledMethod)
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
            Expr InitializerExpr = BuildExpression<Expr>(verificationContext, Argument.Expression);
            BoolExpr InitExpr = Context.MkEq(TemporaryLocalExpr, InitializerExpr);

            AddToSolver(solver, branch, InitExpr);
        }

        VerificationContext CallVerificationContext = verificationContext with { HostMethod = calledMethod, ResultLocal = null };

        if (!AddMethodRequires(CallVerificationContext, solver, checkOpposite: true))
            return false;

        if (!AddStatementListExecution(CallVerificationContext, solver, branch, calledMethod.StatementList))
            return false;

        if (!AddMethodEnsures(CallVerificationContext, solver, keepNormal: true))
            return false;

        return true;
    }

    private void AddReturnExecution(VerificationContext verificationContext, Solver solver, BoolExpr branch, ReturnStatement returnStatement)
    {
        Expression? ReturnExpression = (Expression?)returnStatement.Expression;

        if (ReturnExpression is not null)
        {
            Debug.Assert(verificationContext.HostMethod is not null);
            Debug.Assert(verificationContext.ResultLocal is not null);

            Method HostMethod = verificationContext.HostMethod!;
            Local ResultLocal = verificationContext.ResultLocal!;
            AliasTable AliasTable = verificationContext.AliasTable;

            Expr ResultInitializerExpr = BuildExpression<Expr>(verificationContext, ReturnExpression);

            LocalName ResultLocalBlockName = CreateLocalBlockName(HostMethod, ResultLocal);
            Variable ResultLocalVariable = new(ResultLocalBlockName, ResultLocal.Type);

            AliasTable.IncrementAlias(ResultLocalVariable);
            VariableAlias ResultLocalAlias = AliasTable.GetAlias(ResultLocalVariable);
            Expr ResultLocalExpr = CreateVariableExpr(ResultLocalAlias.ToString(), ResultLocal.Type);

            AddToSolver(solver, branch, Context.MkEq(ResultLocalExpr, ResultInitializerExpr));
        }
    }
}
