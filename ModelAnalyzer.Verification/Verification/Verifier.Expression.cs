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
            case LiteralNullExpression LiteralNull:
                Result = BuildLiteralNullExpression(LiteralNull, out Expr LiteralNullExpr);
                ResultExpr = LiteralNullExpr;
                break;
            case VariableValueExpression VariableValue:
                Result = BuildVariableValueExpression(verificationContext, VariableValue, out Expr VariableValueExpr);
                ResultExpr = VariableValueExpr;
                break;
            case FunctionCallExpression FunctionCall:
                Result = BuildFunctionCallExpression(verificationContext, FunctionCall, out Expr FunctionCallExpr);
                ResultExpr = FunctionCallExpr;
                break;
            case NewObjectExpression NewObject:
                Result = BuildNewObjectExpression(verificationContext, NewObject, out Expr NewObjectExpr);
                ResultExpr = NewObjectExpr;
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
            BoolExpr AssertionExpr = Context.CreateNotEqualExpr(Right, Context.Zero);
            if (!AddMethodAssertionOpposite(verificationContext, AssertionExpr, index: -1, binaryArithmeticExpression.ToString(), VerificationErrorType.AssumeError))
            {
                resultExpr = Context.Zero;
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

        BoolExpr AssertionExpr = Context.CreateNotEqualExpr(Right, Context.Zero);
        if (!AddMethodAssertionOpposite(verificationContext, AssertionExpr, index: -1, remainderExpression.ToString(), VerificationErrorType.AssumeError))
        {
            resultExpr = Context.Zero;
            return false;
        }

        resultExpr = Context.CreateRemainderExpr(Left, Right);

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
        resultExpr = Context.CreateBooleanValue(literalBooleanValueExpression.Value);
        return true;
    }

    private bool BuildLiteralIntegerValueExpression(LiteralIntegerValueExpression literalIntegerValueExpression, out IntExpr resultExpr)
    {
        resultExpr = Context.CreateIntegerValue(literalIntegerValueExpression.Value);
        return true;
    }

    private bool BuildLiteralFloatingPointValueExpression(LiteralFloatingPointValueExpression literalFloatingPointValueExpression, out ArithExpr resultExpr)
    {
        resultExpr = Context.CreateFloatingPointValue(literalFloatingPointValueExpression.Value);
        return true;
    }

    private bool BuildLiteralNullExpression(LiteralNullExpression literalNullExpression, out Expr resultExpr)
    {
        resultExpr = Context.Null;
        return true;
    }

    private bool BuildVariableValueExpression(VerificationContext verificationContext, VariableValueExpression variableValueExpression, out Expr resultExpr)
    {
        resultExpr = null!;

        string VariableName = variableValueExpression.VariableName.Text;
        VariableAlias? VariableAlias = null;

        foreach (KeyValuePair<PropertyName, Property> Entry in verificationContext.PropertyTable)
            if (Entry.Key.Text == VariableName)
            {
                Property Property = Entry.Value;
                resultExpr = verificationContext.ObjectManager.CreateValueExpr(hostMethod: null, Property.Name, Property.Type);
                break;
            }

        foreach (KeyValuePair<FieldName, Field> Entry in verificationContext.FieldTable)
            if (Entry.Key.Text == VariableName)
            {
                Field Field = Entry.Value;
                resultExpr = verificationContext.ObjectManager.CreateValueExpr(hostMethod: null, Field.Name, Field.Type);
                break;
            }

        if (verificationContext.HostMethod is Method HostMethod)
        {
            ReadOnlyParameterTable ParameterTable = HostMethod.ParameterTable;

            foreach (KeyValuePair<ParameterName, Parameter> Entry in ParameterTable)
                if (Entry.Key.Text == VariableName)
                {
                    Parameter Parameter = Entry.Value;
                    resultExpr = verificationContext.ObjectManager.CreateValueExpr(HostMethod, Parameter.Name, Parameter.Type);
                    break;
                }

            ReadOnlyLocalTable LocalTable = HostMethod.LocalTable;

            foreach (KeyValuePair<LocalName, Local> Entry in LocalTable)
                if (Entry.Key.Text == VariableName)
                {
                    Local Local = Entry.Value;
                    resultExpr = verificationContext.ObjectManager.CreateValueExpr(HostMethod, Local.Name, Local.Type);
                    break;
                }

            if (VariableAlias is null && verificationContext.ResultLocal is Local ResultLocal && VariableName == Ensure.ResultKeyword)
                resultExpr = verificationContext.ObjectManager.CreateValueExpr(HostMethod, ResultLocal.Name, ResultLocal.Type);
        }

        Debug.Assert(resultExpr is not null);

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
        resultExpr = verificationContext.ObjectManager.GetDefaultExpr(calledFunction.ReturnType);
        List<Argument> ArgumentList = functionCallExpression.ArgumentList;

        int Index = 0;
        foreach (var Entry in calledFunction.ParameterTable)
        {
            Argument Argument = ArgumentList[Index++];
            Parameter Parameter = Entry.Value;

            if (!BuildExpression(verificationContext, Argument.Expression, out Expr InitializerExpr))
                return false;

            verificationContext.ObjectManager.CreateVariable(calledFunction, Parameter.Name, Parameter.Type, verificationContext.Branch, InitializerExpr);
        }

        LocalName ResultLocalName = new LocalName() { Text = CreateTemporaryResultLocal() };
        Local ResultLocal = new Local() { Name = ResultLocalName, Type = calledFunction.ReturnType, Initializer = null };
        verificationContext.ObjectManager.CreateVariable(calledFunction, ResultLocal.Name, ResultLocal.Type, branch: null, initializerExpr: null);

        VerificationContext CallVerificationContext = verificationContext with { HostMethod = calledFunction, ResultLocal = ResultLocal };

        if (!AddMethodRequires(CallVerificationContext, checkOpposite: true))
            return false;

        if (!AddStatementListExecution(CallVerificationContext, calledFunction.StatementList))
            return false;

        if (!AddMethodEnsures(CallVerificationContext, keepNormal: true))
            return false;

        resultExpr = verificationContext.ObjectManager.CreateValueExpr(calledFunction, ResultLocal.Name, ResultLocal.Type);

        return true;
    }

    private string CreateTemporaryResultLocal()
    {
        string Result = $"temp_{TemporaryResultIndex}";
        TemporaryResultIndex++;

        return Result;
    }

    private bool BuildNewObjectExpression(VerificationContext verificationContext, NewObjectExpression newObjectExpression, out Expr resultExpr)
    {
        bool Result = true;
        resultExpr = verificationContext.ObjectManager.CreateObjectInitializer(newObjectExpression);

        return Result;
    }

    private static int TemporaryResultIndex;
}
