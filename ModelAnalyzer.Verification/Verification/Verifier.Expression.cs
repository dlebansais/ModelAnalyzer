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
    private bool BuildExpression<T>(VerificationContext verificationContext, IExpression expression, out T expr)
        where T : Expr
    {
        bool Result = false;
        Expr? ResultExpr = null;

        switch (expression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                Result = BuildBinaryArithmeticExpression(verificationContext, BinaryArithmetic, out ArithExpr BinaryArithmeticExpr);
                ResultExpr = BinaryArithmeticExpr;
                break;
            case RemainderExpression Remainder:
                Result = BuildRemainderExpression(verificationContext, Remainder, out IntExpr RemainderExpr);
                ResultExpr = RemainderExpr;
                break;
            case UnaryArithmeticExpression UnaryArithmetic:
                Result = BuildUnaryArithmeticExpression(verificationContext, UnaryArithmetic, out ArithExpr UnaryArithmeticExpr);
                ResultExpr = UnaryArithmeticExpr;
                break;
            case BinaryLogicalExpression BinaryLogical:
                Result = BuildBinaryLogicalExpression(verificationContext, BinaryLogical, out BoolExpr BinaryLogicalExpr);
                ResultExpr = BinaryLogicalExpr;
                break;
            case UnaryLogicalExpression UnaryLogical:
                Result = BuildUnaryLogicalExpression(verificationContext, UnaryLogical, out BoolExpr UnaryLogicalExpr);
                ResultExpr = UnaryLogicalExpr;
                break;
            case EqualityExpression Equality:
                Result = BuildEqualityExpression(verificationContext, Equality, out BoolExpr EqualityExpr);
                ResultExpr = EqualityExpr;
                break;
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(verificationContext, Comparison, out BoolExpr ComparisonExpr);
                ResultExpr = ComparisonExpr;
                break;
            case LiteralBooleanValueExpression LiteralBooleanValue:
                Result = BuildLiteralBooleanValueExpression(LiteralBooleanValue, out BoolExpr LiteralBooleanValueExpr);
                ResultExpr = LiteralBooleanValueExpr;
                break;
            case LiteralIntegerValueExpression LiteralIntegerValue:
                Result = BuildLiteralIntegerValueExpression(LiteralIntegerValue, out IntExpr LiteralIntegerValueExpr);
                ResultExpr = LiteralIntegerValueExpr;
                break;
            case LiteralFloatingPointValueExpression LiteralFloatingPointValue:
                Result = BuildLiteralFloatingPointValueExpression(LiteralFloatingPointValue, out ArithExpr LiteralFloatingPointValueExpr);
                ResultExpr = LiteralFloatingPointValueExpr;
                break;
            case VariableValueExpression VariableValue:
                Result = BuildVariableValueExpression(verificationContext, VariableValue, out Expr VariableValueExpr);
                ResultExpr = VariableValueExpr;
                break;
            case FunctionCallExpression FunctionCall:
                Result = BuildFunctionCallExpression(verificationContext, FunctionCall, out Expr FunctionCallExpr);
                ResultExpr = FunctionCallExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = (T)ResultExpr!;
        return Result;
    }

    private bool BuildBinaryArithmeticExpression(VerificationContext verificationContext, BinaryArithmeticExpression binaryArithmeticExpression, out ArithExpr resultExpr)
    {
        bool ResultLeft = BuildExpression(verificationContext, binaryArithmeticExpression.Left, out ArithExpr Left);
        bool ResultRight = BuildExpression(verificationContext, binaryArithmeticExpression.Right, out ArithExpr Right);

        if (binaryArithmeticExpression.Operator == BinaryArithmeticOperator.Divide)
        {
            BoolExpr AssertionExpr = Context.MkNot(Context.MkEq(Right, Zero));
            if (!AddMethodAssertionOpposite(verificationContext, AssertionExpr, index: -1, binaryArithmeticExpression.ToString(), VerificationErrorType.AssumeError))
            {
                resultExpr = Zero;
                return false;
            }
        }

        resultExpr = OperatorBuilder.BinaryArithmetic[binaryArithmeticExpression.Operator](Context, Left, Right);

        return ResultLeft && ResultRight;
    }

    private bool BuildRemainderExpression(VerificationContext verificationContext, RemainderExpression remainderExpression, out IntExpr resultExpr)
    {
        bool ResultLeft = BuildExpression(verificationContext, remainderExpression.Left, out IntExpr Left);
        bool ResultRight = BuildExpression(verificationContext, remainderExpression.Right, out IntExpr Right);

        BoolExpr AssertionExpr = Context.MkNot(Context.MkEq(Right, Zero));
        if (!AddMethodAssertionOpposite(verificationContext, AssertionExpr, index: -1, remainderExpression.ToString(), VerificationErrorType.AssumeError))
        {
            resultExpr = Zero;
            return false;
        }

        resultExpr = Context.MkMod(Left, Right);

        return ResultLeft && ResultRight;
    }

    private bool BuildUnaryArithmeticExpression(VerificationContext verificationContext, UnaryArithmeticExpression unaryArithmeticExpression, out ArithExpr resultExpr)
    {
        bool ResultOperand = BuildExpression(verificationContext, unaryArithmeticExpression.Operand, out ArithExpr Operand);

        resultExpr = OperatorBuilder.UnaryArithmetic[unaryArithmeticExpression.Operator](Context, Operand);

        return ResultOperand;
    }

    private bool BuildBinaryLogicalExpression(VerificationContext verificationContext, BinaryLogicalExpression binaryLogicalExpression, out BoolExpr resultExpr)
    {
        bool ResultLeft = BuildExpression(verificationContext, binaryLogicalExpression.Left, out BoolExpr Left);
        bool ResultRight = BuildExpression(verificationContext, binaryLogicalExpression.Right, out BoolExpr Right);

        resultExpr = OperatorBuilder.BinaryLogical[binaryLogicalExpression.Operator](Context, Left, Right);

        return ResultLeft && ResultRight;
    }

    private bool BuildUnaryLogicalExpression(VerificationContext verificationContext, UnaryLogicalExpression unaryLogicalExpression, out BoolExpr resultExpr)
    {
        bool ResultOperand = BuildExpression(verificationContext, unaryLogicalExpression.Operand, out BoolExpr Operand);

        resultExpr = OperatorBuilder.UnaryLogical[unaryLogicalExpression.Operator](Context, Operand);

        return ResultOperand;
    }

    private bool BuildEqualityExpression(VerificationContext verificationContext, EqualityExpression equalityExpression, out BoolExpr resultExpr)
    {
        bool ResultLeft = BuildExpression(verificationContext, equalityExpression.Left, out Expr Left);
        bool ResultRight = BuildExpression(verificationContext, equalityExpression.Right, out Expr Right);

        resultExpr = OperatorBuilder.Equality[equalityExpression.Operator](Context, Left, Right);

        return ResultLeft && ResultRight;
    }

    private bool BuildComparisonExpression(VerificationContext verificationContext, ComparisonExpression comparisonExpression, out BoolExpr resultExpr)
    {
        bool ResultLeft = BuildExpression<ArithExpr>(verificationContext, comparisonExpression.Left, out ArithExpr Left);
        bool ResultRight = BuildExpression<ArithExpr>(verificationContext, comparisonExpression.Right, out ArithExpr Right);

        resultExpr = OperatorBuilder.Comparison[comparisonExpression.Operator](Context, Left, Right);

        return ResultLeft && ResultRight;
    }

    private bool BuildLiteralBooleanValueExpression(LiteralBooleanValueExpression literalBooleanValueExpression, out BoolExpr resultExpr)
    {
        resultExpr = CreateBooleanExpr(literalBooleanValueExpression.Value);
        return true;
    }

    private bool BuildLiteralIntegerValueExpression(LiteralIntegerValueExpression literalIntegerValueExpression, out IntExpr resultExpr)
    {
        resultExpr = CreateIntegerExpr(literalIntegerValueExpression.Value);
        return true;
    }

    private bool BuildLiteralFloatingPointValueExpression(LiteralFloatingPointValueExpression literalFloatingPointValueExpression, out ArithExpr resultExpr)
    {
        resultExpr = CreateFloatingPointExpr(literalFloatingPointValueExpression.Value);
        return true;
    }

    private bool BuildVariableValueExpression(VerificationContext verificationContext, VariableValueExpression variableValueExpression, out Expr resultExpr)
    {
        AliasTable AliasTable = verificationContext.AliasTable;
        string VariableName = variableValueExpression.VariableName.Text;
        string? VariableString = null;
        ExpressionType VariableType = variableValueExpression.GetExpressionType(verificationContext);

        foreach (KeyValuePair<PropertyName, Property> Entry in verificationContext.PropertyTable)
            if (Entry.Key.Text == VariableName)
            {
                Property Property = Entry.Value;
                Variable PropertyVariable = new(Property.Name, Property.Type);
                VariableAlias PropertyAlias = AliasTable.GetAlias(PropertyVariable);
                VariableString = PropertyAlias.ToString();
                break;
            }

        foreach (KeyValuePair<FieldName, Field> Entry in verificationContext.FieldTable)
            if (Entry.Key.Text == VariableName)
            {
                Field Field = Entry.Value;
                Variable FieldVariable = new(Field.Name, Field.Type);
                VariableAlias FieldAlias = AliasTable.GetAlias(FieldVariable);
                VariableString = FieldAlias.ToString();
                break;
            }

        if (verificationContext.HostMethod is Method HostMethod)
        {
            ReadOnlyParameterTable ParameterTable = HostMethod.ParameterTable;

            foreach (KeyValuePair<ParameterName, Parameter> Entry in ParameterTable)
                if (Entry.Key.Text == VariableName)
                {
                    Parameter Parameter = Entry.Value;
                    ParameterName ParameterBlockName = CreateParameterBlockName(HostMethod, Parameter);
                    Variable ParameterVariable = new(ParameterBlockName, Parameter.Type);
                    VariableAlias ParameterAlias = AliasTable.GetAlias(ParameterVariable);
                    VariableString = ParameterAlias.ToString();
                    break;
                }

            ReadOnlyLocalTable LocalTable = HostMethod.LocalTable;

            foreach (KeyValuePair<LocalName, Local> Entry in LocalTable)
                if (Entry.Key.Text == VariableName)
                {
                    Local Local = Entry.Value;
                    LocalName LocalBlockName = CreateLocalBlockName(HostMethod, Local);
                    Variable LocalVariable = new(LocalBlockName, Local.Type);
                    VariableAlias LocalAlias = AliasTable.GetAlias(LocalVariable);
                    VariableString = LocalAlias.ToString();
                    break;
                }

            if (VariableString is null && verificationContext.ResultLocal is Local ResultLocal && VariableName == Ensure.ResultKeyword)
            {
                LocalName ResultLocalBlockName = CreateLocalBlockName(HostMethod, ResultLocal);
                Variable ResultLocalVariable = new(ResultLocalBlockName, ResultLocal.Type);
                VariableAlias ResultLocalAlias = AliasTable.GetAlias(ResultLocalVariable);
                VariableString = ResultLocalAlias.ToString();
            }
        }

        Debug.Assert(VariableString is not null);
        Debug.Assert(VariableType != ExpressionType.Other);

        resultExpr = CreateVariableExpr(verificationContext, VariableString!, VariableType);

        return true;
    }

    private bool BuildFunctionCallExpression(VerificationContext verificationContext, FunctionCallExpression functionCallExpression, out Expr resultExpr)
    {
        bool Result = false;
        resultExpr = null!;

        foreach (var Entry in MethodTable)
            if (Entry.Key == functionCallExpression.FunctionName)
            {
                Method CalledFunction = Entry.Value;
                Result = BuildFunctionCallExpression(verificationContext, functionCallExpression, CalledFunction, out resultExpr);
                break;
            }

        Debug.Assert(resultExpr is not null);

        return Result;
    }

    private bool BuildFunctionCallExpression(VerificationContext verificationContext, FunctionCallExpression functionCallExpression, Method calledFunction, out Expr resultExpr)
    {
        resultExpr = GetDefaultExpr(calledFunction.ReturnType);
        AliasTable AliasTable = verificationContext.AliasTable;
        List<Argument> ArgumentList = functionCallExpression.ArgumentList;

        int Index = 0;
        foreach (var Entry in calledFunction.ParameterTable)
        {
            Argument Argument = ArgumentList[Index++];
            Parameter Parameter = Entry.Value;
            ParameterName ParameterBlockName = CreateParameterBlockName(calledFunction, Parameter);
            Variable ParameterVariable = new(ParameterBlockName, Parameter.Type);

            AliasTable.AddOrIncrement(ParameterVariable);
            VariableAlias ParameterNameAlias = AliasTable.GetAlias(ParameterVariable);

            Expr TemporaryLocalExpr = CreateVariableExpr(verificationContext, ParameterNameAlias.ToString(), Parameter.Type);

            if (!BuildExpression(verificationContext, Argument.Expression, out Expr InitializerExpr))
                return false;

            BoolExpr InitExpr = Context.MkEq(TemporaryLocalExpr, InitializerExpr);

            AddToSolver(verificationContext, InitExpr);
        }

        LocalName ResultLocalName = new LocalName() { Text = CreateTemporaryResultLocal() };
        Local ResultLocal = new Local() { Name = ResultLocalName, Type = calledFunction.ReturnType, Initializer = null };
        LocalName ResultLocalBlockName = CreateLocalBlockName(calledFunction, ResultLocal);
        Variable ResultLocalVariable = new(ResultLocalBlockName, calledFunction.ReturnType);
        AliasTable.AddVariable(ResultLocalVariable);

        VerificationContext CallVerificationContext = verificationContext with { HostMethod = calledFunction, ResultLocal = ResultLocal };

        if (!AddMethodRequires(CallVerificationContext, checkOpposite: true))
            return false;

        if (!AddStatementListExecution(CallVerificationContext, calledFunction.StatementList))
            return false;

        if (!AddMethodEnsures(CallVerificationContext, keepNormal: true))
            return false;

        VariableAlias ResultLocalAlias = AliasTable.GetAlias(ResultLocalVariable);
        string ResultLocalString = ResultLocalAlias.ToString();
        resultExpr = CreateVariableExpr(verificationContext, ResultLocalString, calledFunction.ReturnType);

        return true;
    }

    private string CreateTemporaryResultLocal()
    {
        string Result = $"temp_{TemporaryResultIndex}";
        TemporaryResultIndex++;

        return Result;
    }

    private static int TemporaryResultIndex;
}
