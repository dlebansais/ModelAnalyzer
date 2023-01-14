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
            case VariableValueExpression VariableValue:
                Result = BuildVariableValueExpression(verificationContext, VariableValue, out Expr VariableValueExpr);
                ResultExpr = VariableValueExpr;
                break;
            case FunctionCallExpression FunctionCall:
                Result = BuildFunctionCallExpression(verificationContext, FunctionCall, out Expr FunctionCallExpr);
                ResultExpr = FunctionCallExpr;
                break;
            case IBinaryExpression Binary:
                Result = BuildBinaryExpression(verificationContext, Binary, out Expr BinaryExpr);
                ResultExpr = BinaryExpr;
                break;
            case IUnaryExpression Unary:
                Result = BuildUnaryExpression(verificationContext, Unary, out Expr unaryExpr);
                ResultExpr = unaryExpr;
                break;
            case ILiteralExpression Literal:
                Result = BuildLiteralValueExpression(verificationContext, Literal, out Expr LiteralExpr);
                ResultExpr = LiteralExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = (T)ResultExpr!;
        return Result;
    }

    private bool BuildVariableValueExpression(VerificationContext verificationContext, VariableValueExpression variableValueExpression, out Expr resultExpr)
    {
        string Name = variableValueExpression.VariableName.Text;
        Method? HostMethod = null;
        IVariableName? VariableName = null;
        ExpressionType? VariableType = null;

        LookupProperty(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);
        LookupField(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);
        LookupParameter(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);
        LookupLocal(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);

        Debug.Assert(VariableName is not null);
        Debug.Assert(VariableType is not null);

        resultExpr = verificationContext.ObjectManager.CreateValueExpr(HostMethod is null ? ObjectManager.ThisObject : null, HostMethod, VariableName!, VariableType!);

        return true;
    }

    private void LookupProperty(VerificationContext verificationContext, string name, ref Method? hostMethod, ref IVariableName? variableName, ref ExpressionType? variableType)
    {
        foreach (KeyValuePair<PropertyName, Property> Entry in verificationContext.PropertyTable)
            if (Entry.Key.Text == name)
            {
                Property Property = Entry.Value;

                Debug.Assert(hostMethod is null);
                Debug.Assert(variableName is null);
                Debug.Assert(variableType is null);

                hostMethod = null;
                variableName = Property.Name;
                variableType = Property.Type;
                break;
            }
    }

    private void LookupField(VerificationContext verificationContext, string name, ref Method? hostMethod, ref IVariableName? variableName, ref ExpressionType? variableType)
    {
        foreach (KeyValuePair<FieldName, Field> Entry in verificationContext.FieldTable)
            if (Entry.Key.Text == name)
            {
                Field Field = Entry.Value;

                Debug.Assert(hostMethod is null);
                Debug.Assert(variableName is null);
                Debug.Assert(variableType is null);

                hostMethod = null;
                variableName = Field.Name;
                variableType = Field.Type;
                break;
            }
    }

    private void LookupParameter(VerificationContext verificationContext, string name, ref Method? hostMethod, ref IVariableName? variableName, ref ExpressionType? variableType)
    {
        if (verificationContext.HostMethod is null)
            return;

        ReadOnlyParameterTable ParameterTable = verificationContext.HostMethod.ParameterTable;

        foreach (KeyValuePair<ParameterName, Parameter> Entry in ParameterTable)
            if (Entry.Key.Text == name)
            {
                Parameter Parameter = Entry.Value;

                Debug.Assert(hostMethod is null);
                Debug.Assert(variableName is null);
                Debug.Assert(variableType is null);

                variableName = Parameter.Name;
                variableType = Parameter.Type;
                hostMethod = verificationContext.HostMethod;

                Debug.Assert(hostMethod is not null);
                break;
            }
    }

    private void LookupLocal(VerificationContext verificationContext, string name, ref Method? hostMethod, ref IVariableName? variableName, ref ExpressionType? variableType)
    {
        if (verificationContext.HostMethod is null)
            return;

        ReadOnlyLocalTable LocalTable = verificationContext.HostMethod.LocalTable;

        foreach (KeyValuePair<LocalName, Local> Entry in LocalTable)
            if (Entry.Key.Text == name)
            {
                Local Local = Entry.Value;

                Debug.Assert(hostMethod is null);
                Debug.Assert(variableName is null);
                Debug.Assert(variableType is null);

                variableName = Local.Name;
                variableType = Local.Type;
                hostMethod = verificationContext.HostMethod;

                Debug.Assert(hostMethod is not null);
                return;
            }

        if (verificationContext.ResultLocal is Local ResultLocal && name == Ensure.ResultKeyword)
        {
            Debug.Assert(hostMethod is null);
            Debug.Assert(variableName is null);
            Debug.Assert(variableType is null);

            variableName = ResultLocal.Name;
            variableType = ResultLocal.Type;
            hostMethod = verificationContext.HostMethod;

            Debug.Assert(hostMethod is not null);
        }
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

            verificationContext.ObjectManager.CreateVariable(owner: null, calledFunction, Parameter.Name, Parameter.Type, verificationContext.Branch, InitializerExpr);
        }

        LocalName ResultLocalName = new LocalName() { Text = CreateTemporaryResultLocal() };
        Local ResultLocal = new Local() { Name = ResultLocalName, Type = calledFunction.ReturnType, Initializer = null };
        verificationContext.ObjectManager.CreateVariable(owner: null, calledFunction, ResultLocal.Name, ResultLocal.Type, branch: null, initializerExpr: null);

        VerificationContext CallVerificationContext = verificationContext with { HostMethod = calledFunction, ResultLocal = ResultLocal };

        if (!AddMethodRequires(CallVerificationContext, checkOpposite: true))
            return false;

        if (!AddStatementListExecution(CallVerificationContext, calledFunction.StatementList))
            return false;

        if (!AddMethodEnsures(CallVerificationContext, keepNormal: true))
            return false;

        resultExpr = verificationContext.ObjectManager.CreateValueExpr(owner: null, calledFunction, ResultLocal.Name, ResultLocal.Type);

        return true;
    }

    private string CreateTemporaryResultLocal()
    {
        string Result = $"temp_{TemporaryResultIndex}";
        TemporaryResultIndex++;

        return Result;
    }

    private bool BuildBinaryExpression(VerificationContext verificationContext, IBinaryExpression binaryExpression, out Expr expr)
    {
        bool Result = false;
        Expr? ResultExpr = null;

        switch (binaryExpression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                Result = BuildBinaryArithmeticExpression(verificationContext, BinaryArithmetic, out ArithExpr BinaryArithmeticExpr);
                ResultExpr = BinaryArithmeticExpr;
                break;
            case RemainderExpression Remainder:
                Result = BuildRemainderExpression(verificationContext, Remainder, out IntExpr RemainderExpr);
                ResultExpr = RemainderExpr;
                break;
            case BinaryLogicalExpression BinaryLogical:
                Result = BuildBinaryLogicalExpression(verificationContext, BinaryLogical, out BoolExpr BinaryLogicalExpr);
                ResultExpr = BinaryLogicalExpr;
                break;
            case EqualityExpression Equality:
                Result = BuildEqualityExpression(verificationContext, Equality, out BoolExpr EqualityExpr);
                ResultExpr = EqualityExpr;
                break;
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(verificationContext, Comparison, out BoolExpr ComparisonExpr);
                ResultExpr = ComparisonExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = ResultExpr!;
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

    private bool BuildBinaryLogicalExpression(VerificationContext verificationContext, BinaryLogicalExpression binaryLogicalExpression, out BoolExpr resultExpr)
    {
        bool ResultLeft = BuildExpression(verificationContext, binaryLogicalExpression.Left, out BoolExpr Left);
        bool ResultRight = BuildExpression(verificationContext, binaryLogicalExpression.Right, out BoolExpr Right);

        resultExpr = OperatorBuilder.BinaryLogical[binaryLogicalExpression.Operator](Context, Left, Right);

        return ResultLeft && ResultRight;
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

    private bool BuildUnaryExpression(VerificationContext verificationContext, IUnaryExpression unaryExpression, out Expr expr)
    {
        bool Result = false;
        Expr? ResultExpr = null;

        switch (unaryExpression)
        {
            case UnaryArithmeticExpression UnaryArithmetic:
                Result = BuildUnaryArithmeticExpression(verificationContext, UnaryArithmetic, out ArithExpr UnaryArithmeticExpr);
                ResultExpr = UnaryArithmeticExpr;
                break;
            case UnaryLogicalExpression UnaryLogical:
                Result = BuildUnaryLogicalExpression(verificationContext, UnaryLogical, out BoolExpr UnaryLogicalExpr);
                ResultExpr = UnaryLogicalExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = ResultExpr!;
        return Result;
    }

    private bool BuildUnaryArithmeticExpression(VerificationContext verificationContext, UnaryArithmeticExpression unaryArithmeticExpression, out ArithExpr resultExpr)
    {
        bool ResultOperand = BuildExpression(verificationContext, unaryArithmeticExpression.Operand, out ArithExpr Operand);

        resultExpr = OperatorBuilder.UnaryArithmetic[unaryArithmeticExpression.Operator](Context, Operand);

        return ResultOperand;
    }

    private bool BuildUnaryLogicalExpression(VerificationContext verificationContext, UnaryLogicalExpression unaryLogicalExpression, out BoolExpr resultExpr)
    {
        bool ResultOperand = BuildExpression(verificationContext, unaryLogicalExpression.Operand, out BoolExpr Operand);

        resultExpr = OperatorBuilder.UnaryLogical[unaryLogicalExpression.Operator](Context, Operand);

        return ResultOperand;
    }

    private bool BuildLiteralValueExpression(VerificationContext verificationContext, ILiteralExpression literalExpression, out Expr expr)
    {
        bool Result = false;
        Expr? ResultExpr = null;

        switch (literalExpression)
        {
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
            case NewObjectExpression NewObject:
                Result = BuildNewObjectExpression(verificationContext, NewObject, out Expr NewObjectExpr);
                ResultExpr = NewObjectExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = ResultExpr!;
        return Result;
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

    private bool BuildNewObjectExpression(VerificationContext verificationContext, NewObjectExpression newObjectExpression, out Expr resultExpr)
    {
        bool Result = true;
        resultExpr = verificationContext.ObjectManager.CreateObjectInitializer(newObjectExpression);

        return Result;
    }

    private static int TemporaryResultIndex;
}
