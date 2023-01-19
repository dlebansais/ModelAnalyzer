namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Z3;

/// <summary>
/// Represents a code verifier.
/// </summary>
internal partial class Verifier : IDisposable
{
    private bool BuildExpression<T>(VerificationContext verificationContext, IExpression expression, out IExprSet<T> expr)
        where T : IExprCapsule
    {
        bool Result = false;
        IExprSet<IExprCapsule>? ResultExpr = null;

        switch (expression)
        {
            case VariableValueExpression VariableValue:
                Result = BuildVariableValueExpression(verificationContext, VariableValue, out IExprSet<IExprCapsule> VariableValueExpr);
                ResultExpr = VariableValueExpr;
                break;
            case PrivateFunctionCallExpression PrivateFunctionCall:
                Result = BuildPrivateFunctionCallExpression(verificationContext, PrivateFunctionCall, out IExprSet<IExprCapsule> PrivateFunctionCallExpr);
                ResultExpr = PrivateFunctionCallExpr;
                break;
            case PublicFunctionCallExpression PublicFunctionCall:
                Result = BuildPublicFunctionCallExpression(verificationContext, PublicFunctionCall, out IExprSet<IExprCapsule> PublicFunctionCallExpr);
                ResultExpr = PublicFunctionCallExpr;
                break;
            case IBinaryExpression Binary:
                Result = BuildBinaryExpression(verificationContext, Binary, out IExprSet<IExprCapsule> BinaryExpr);
                ResultExpr = BinaryExpr;
                break;
            case IUnaryExpression Unary:
                Result = BuildUnaryExpression(verificationContext, Unary, out IExprSet<IExprCapsule> unaryExpr);
                ResultExpr = unaryExpr;
                break;
            case ILiteralExpression Literal:
                Result = BuildLiteralValueExpression(verificationContext, Literal, out IExprSet<IExprCapsule> LiteralExpr);
                ResultExpr = LiteralExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = (IExprSet<T>)ResultExpr!;
        return Result;
    }

    /*
    private bool BuildVariableValueExpression(VerificationContext verificationContext, VariableValueExpression variableValueExpression, out IExprSet<IExprCapsule> resultExpr)
    {
        List<IVariable> VariablePath = variableValueExpression.VariablePath;
        Debug.Assert(VariablePath.Count >= 1);

        string Name = VariablePath.First().Name.Text;
        Method? HostMethod = null;
        IVariableName? VariableName = null;
        ExpressionType? VariableType = null;

        LookupProperty(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);
        LookupField(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);
        LookupParameter(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);
        LookupLocal(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);

        Debug.Assert(VariableName is not null);
        Debug.Assert(VariableType is not null);

        resultExpr = verificationContext.ObjectManager.CreateValueExpr(HostMethod is null ? verificationContext.ObjectManager.ThisObject : null, HostMethod, VariableName!, VariableType!);

        for (int i = 1; i < VariablePath.Count; i++)
        {
            Debug.Assert(VariablePath[i] is IProperty);
            IProperty Property = (IProperty)VariablePath[i];

            Debug.Assert(resultExpr.MainExpression is IRefExprCapsule);
            IRefExprCapsule Reference = (IRefExprCapsule)resultExpr.MainExpression;

            resultExpr = verificationContext.ObjectManager.CreateValueExpr(Reference, hostMethod: null, Property.Name, Property.Type);
        }

        return true;
    }*/

    private bool BuildVariableValueExpression(VerificationContext verificationContext, VariableValueExpression variableValueExpression, out IExprSet<IExprCapsule> resultExpr)
    {
        return BuildVariableValueExpression(verificationContext, variableValueExpression.VariablePath, pathIndex: 0, owner: verificationContext.ObjectManager.ThisObject, out resultExpr);
    }

    private bool BuildVariableValueExpression(VerificationContext verificationContext, List<IVariable> variablePath, int pathIndex, IRefExprCapsule? owner, out IExprSet<IExprCapsule> resultExpr)
    {
        Debug.Assert(pathIndex < variablePath.Count);

        string Name = variablePath[pathIndex].Name.Text;
        Method? HostMethod = null;
        IVariableName? VariableName = null;
        ExpressionType? VariableType = null;

        LookupProperty(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);

        if (pathIndex == 0)
        {
            LookupField(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);
            LookupParameter(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);
            LookupLocal(verificationContext, Name, ref HostMethod, ref VariableName, ref VariableType);
        }

        Debug.Assert(VariableName is not null);
        Debug.Assert(VariableType is not null);

        resultExpr = verificationContext.ObjectManager.CreateValueExpr(HostMethod is null ? owner : null, HostMethod, VariableName!, VariableType!);

        if (pathIndex + 1 < variablePath.Count)
        {
            ClassModel ClassModel = verificationContext.ObjectManager.TypeToModel(VariableType!);
            VerificationContext NewVerificationContext = verificationContext with
            {
                PropertyTable = ClassModel.PropertyTable,
                FieldTable = ReadOnlyFieldTable.Empty,
                MethodTable = ReadOnlyMethodTable.Empty,
                HostMethod = null,
                ResultLocal = null,
            };

            Debug.Assert(resultExpr.MainExpression is IRefExprCapsule);
            IRefExprCapsule NewReference = (IRefExprCapsule)resultExpr.MainExpression;

            return BuildVariableValueExpression(NewVerificationContext, variablePath, pathIndex + 1, NewReference, out resultExpr);
        }
        else
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

    private bool BuildPrivateFunctionCallExpression(VerificationContext verificationContext, PrivateFunctionCallExpression functionCallExpression, out IExprSet<IExprCapsule> resultExpr)
    {
        bool Result = false;
        resultExpr = null!;

        foreach (var Entry in MethodTable)
            if (Entry.Key == functionCallExpression.Name)
            {
                Method CalledFunction = Entry.Value;
                Result = BuildPrivateFunctionCallExpression(verificationContext, functionCallExpression, CalledFunction, out resultExpr);
                break;
            }

        Debug.Assert(resultExpr is not null);

        return Result;
    }

    private bool BuildPrivateFunctionCallExpression(VerificationContext verificationContext, PrivateFunctionCallExpression functionCallExpression, Method calledFunction, out IExprSet<IExprCapsule> resultExpr)
    {
        resultExpr = verificationContext.ObjectManager.GetDefaultExpr(calledFunction.ReturnType);
        List<Argument> ArgumentList = functionCallExpression.ArgumentList;

        int Index = 0;
        foreach (var Entry in calledFunction.ParameterTable)
        {
            Argument Argument = ArgumentList[Index++];
            Parameter Parameter = Entry.Value;

            if (!BuildExpression(verificationContext, Argument.Expression, out IExprSet<IExprCapsule> InitializerExpr))
                return false;

            verificationContext.ObjectManager.CreateVariable(owner: null, calledFunction, Parameter.Name, Parameter.Type, verificationContext.Branch, InitializerExpr);
        }

        LocalName CallResultName = new LocalName() { Text = CreateTemporaryResultLocal() };
        Local CallResult = new Local() { Name = CallResultName, Type = calledFunction.ReturnType, Initializer = null };
        verificationContext.ObjectManager.CreateVariable(owner: null, calledFunction, CallResult.Name, CallResult.Type, branch: null, initializerExpr: null);

        VerificationContext CallVerificationContext = verificationContext with { HostMethod = calledFunction, ResultLocal = CallResult };

        if (!AddMethodRequires(CallVerificationContext, checkOpposite: true))
            return false;

        if (!AddStatementListExecution(CallVerificationContext, calledFunction.StatementList))
            return false;

        if (!AddMethodEnsures(CallVerificationContext, keepNormal: true))
            return false;

        resultExpr = verificationContext.ObjectManager.CreateValueExpr(owner: null, calledFunction, CallResult.Name, CallResult.Type);

        return true;
    }

    private bool BuildPublicFunctionCallExpression(VerificationContext verificationContext, PublicFunctionCallExpression functionCallExpression, out IExprSet<IExprCapsule> resultExpr)
    {
        // TODO
        bool Result = false;
        resultExpr = null!;

        /*
        foreach (var Entry in MethodTable)
            if (Entry.Key == functionCallExpression.Name)
            {
                Method CalledFunction = Entry.Value;
                Result = BuildPublicFunctionCallExpression(verificationContext, functionCallExpression, CalledFunction, out resultExpr);
                break;
            }

        Debug.Assert(resultExpr is not null);
        */

        return Result;
    }

    private bool BuildPublicFunctionCallExpression(VerificationContext verificationContext, PublicFunctionCallExpression functionCallExpression, Method calledFunction, out IExprSet<IExprCapsule> resultExpr)
    {
        // TODO
        resultExpr = verificationContext.ObjectManager.GetDefaultExpr(calledFunction.ReturnType);
        return false;
    }

    private string CreateTemporaryResultLocal()
    {
        string Result = $"temp_{TemporaryResultIndex}";
        TemporaryResultIndex++;

        return Result;
    }

    private bool BuildBinaryExpression(VerificationContext verificationContext, IBinaryExpression binaryExpression, out IExprSet<IExprCapsule> expr)
    {
        bool Result = false;
        IExprSet<IExprCapsule>? ResultExpr = null;

        switch (binaryExpression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                Result = BuildBinaryArithmeticExpression(verificationContext, BinaryArithmetic, out IExprSet<IArithExprCapsule> BinaryArithmeticExpr);
                ResultExpr = BinaryArithmeticExpr;
                break;
            case RemainderExpression Remainder:
                Result = BuildRemainderExpression(verificationContext, Remainder, out IExprSet<IIntExprCapsule> RemainderExpr);
                ResultExpr = RemainderExpr;
                break;
            case BinaryLogicalExpression BinaryLogical:
                Result = BuildBinaryLogicalExpression(verificationContext, BinaryLogical, out IExprSet<IBoolExprCapsule> BinaryLogicalExpr);
                ResultExpr = BinaryLogicalExpr;
                break;
            case EqualityExpression Equality:
                Result = BuildEqualityExpression(verificationContext, Equality, out IExprSet<IBoolExprCapsule> EqualityExpr);
                ResultExpr = EqualityExpr;
                break;
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(verificationContext, Comparison, out IExprSet<IBoolExprCapsule> ComparisonExpr);
                ResultExpr = ComparisonExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = ResultExpr!;
        return Result;
    }

    private bool BuildBinaryArithmeticExpression(VerificationContext verificationContext, BinaryArithmeticExpression binaryArithmeticExpression, out IExprSet<IArithExprCapsule> resultExprSet)
    {
        bool ResultLeft = BuildExpression(verificationContext, binaryArithmeticExpression.Left, out IExprSet<IArithExprCapsule> Left);
        bool ResultRight = BuildExpression(verificationContext, binaryArithmeticExpression.Right, out IExprSet<IArithExprCapsule> Right);

        if (binaryArithmeticExpression.Operator == BinaryArithmeticOperator.Divide)
        {
            IExprSet<IBoolExprCapsule> AssertionExpr = Context.CreateNotEqualExprSet(Right, Context.ZeroSet);
            if (!AddMethodAssertionOpposite(verificationContext, AssertionExpr, index: -1, binaryArithmeticExpression.ToString(), VerificationErrorType.AssumeError))
            {
                resultExprSet = Context.ZeroSet;
                return false;
            }
        }

        Debug.Assert(Left.IsSingle);
        Debug.Assert(Right.IsSingle);

        IArithExprCapsule ResultExpr = OperatorBuilder.BinaryArithmetic[binaryArithmeticExpression.Operator](Context, Left.MainExpression, Right.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultLeft && ResultRight;
    }

    private bool BuildRemainderExpression(VerificationContext verificationContext, RemainderExpression remainderExpression, out IExprSet<IIntExprCapsule> resultExprSet)
    {
        bool ResultLeft = BuildExpression(verificationContext, remainderExpression.Left, out IExprSet<IIntExprCapsule> Left);
        bool ResultRight = BuildExpression(verificationContext, remainderExpression.Right, out IExprSet<IIntExprCapsule> Right);

        IExprSet<IBoolExprCapsule> AssertionExpr = Context.CreateNotEqualExprSet(Right, Context.ZeroSet);
        if (!AddMethodAssertionOpposite(verificationContext, AssertionExpr, index: -1, remainderExpression.ToString(), VerificationErrorType.AssumeError))
        {
            resultExprSet = Context.ZeroSet;
            return false;
        }

        Debug.Assert(Left.IsSingle);
        Debug.Assert(Right.IsSingle);

        IIntExprCapsule ResultExpr = Context.CreateRemainderExpr(Left.MainExpression.Item, Right.MainExpression.Item);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultLeft && ResultRight;
    }

    private bool BuildBinaryLogicalExpression(VerificationContext verificationContext, BinaryLogicalExpression binaryLogicalExpression, out IExprSet<IBoolExprCapsule> resultExprSet)
    {
        bool ResultLeft = BuildExpression(verificationContext, binaryLogicalExpression.Left, out IExprSet<IBoolExprCapsule> Left);
        bool ResultRight = BuildExpression(verificationContext, binaryLogicalExpression.Right, out IExprSet<IBoolExprCapsule> Right);

        Debug.Assert(Left.IsSingle);
        Debug.Assert(Right.IsSingle);

        IBoolExprCapsule ResultExpr = OperatorBuilder.BinaryLogical[binaryLogicalExpression.Operator](Context, Left.MainExpression, Right.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultLeft && ResultRight;
    }

    private bool BuildEqualityExpression(VerificationContext verificationContext, EqualityExpression equalityExpression, out IExprSet<IBoolExprCapsule> resultExprSet)
    {
        bool ResultLeft = BuildExpression(verificationContext, equalityExpression.Left, out IExprSet<IExprCapsule> Left);
        bool ResultRight = BuildExpression(verificationContext, equalityExpression.Right, out IExprSet<IExprCapsule> Right);

        IBoolExprCapsule ResultExpr = OperatorBuilder.Equality[equalityExpression.Operator](Context, Left.MainExpression, Right.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultLeft && ResultRight;
    }

    private bool BuildComparisonExpression(VerificationContext verificationContext, ComparisonExpression comparisonExpression, out IExprSet<IBoolExprCapsule> resultExprSet)
    {
        bool ResultLeft = BuildExpression(verificationContext, comparisonExpression.Left, out IExprSet<IArithExprCapsule> Left);
        bool ResultRight = BuildExpression(verificationContext, comparisonExpression.Right, out IExprSet<IArithExprCapsule> Right);

        Debug.Assert(Left.IsSingle);
        Debug.Assert(Right.IsSingle);

        IBoolExprCapsule ResultExpr = OperatorBuilder.Comparison[comparisonExpression.Operator](Context, Left.MainExpression, Right.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultLeft && ResultRight;
    }

    private bool BuildUnaryExpression(VerificationContext verificationContext, IUnaryExpression unaryExpression, out IExprSet<IExprCapsule> expr)
    {
        bool Result = false;
        IExprSet<IExprCapsule>? ResultExpr = null;

        switch (unaryExpression)
        {
            case UnaryArithmeticExpression UnaryArithmetic:
                Result = BuildUnaryArithmeticExpression(verificationContext, UnaryArithmetic, out IExprSet<IArithExprCapsule> UnaryArithmeticExpr);
                ResultExpr = UnaryArithmeticExpr;
                break;
            case UnaryLogicalExpression UnaryLogical:
                Result = BuildUnaryLogicalExpression(verificationContext, UnaryLogical, out IExprSet<IBoolExprCapsule> UnaryLogicalExpr);
                ResultExpr = UnaryLogicalExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = ResultExpr!;
        return Result;
    }

    private bool BuildUnaryArithmeticExpression(VerificationContext verificationContext, UnaryArithmeticExpression unaryArithmeticExpression, out IExprSet<IArithExprCapsule> resultExprSet)
    {
        bool ResultOperand = BuildExpression(verificationContext, unaryArithmeticExpression.Operand, out IExprSet<IArithExprCapsule> Operand);

        Debug.Assert(Operand.IsSingle);

        IArithExprCapsule ResultExpr = OperatorBuilder.UnaryArithmetic[unaryArithmeticExpression.Operator](Context, Operand.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultOperand;
    }

    private bool BuildUnaryLogicalExpression(VerificationContext verificationContext, UnaryLogicalExpression unaryLogicalExpression, out IExprSet<IBoolExprCapsule> resultExprSet)
    {
        bool ResultOperand = BuildExpression(verificationContext, unaryLogicalExpression.Operand, out IExprSet<IBoolExprCapsule> Operand);

        Debug.Assert(Operand.IsSingle);

        IBoolExprCapsule ResultExpr = OperatorBuilder.UnaryLogical[unaryLogicalExpression.Operator](Context, Operand.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultOperand;
    }

    private bool BuildLiteralValueExpression(VerificationContext verificationContext, ILiteralExpression literalExpression, out IExprSet<IExprCapsule> expr)
    {
        bool Result = false;
        IExprSet<IExprCapsule>? ResultExpr = null;

        switch (literalExpression)
        {
            case LiteralBooleanValueExpression LiteralBooleanValue:
                Result = BuildLiteralBooleanValueExpression(LiteralBooleanValue, out IExprSet<IBoolExprCapsule> LiteralBooleanValueExpr);
                ResultExpr = LiteralBooleanValueExpr;
                break;
            case LiteralIntegerValueExpression LiteralIntegerValue:
                Result = BuildLiteralIntegerValueExpression(LiteralIntegerValue, out IExprSet<IIntExprCapsule> LiteralIntegerValueExpr);
                ResultExpr = LiteralIntegerValueExpr;
                break;
            case LiteralFloatingPointValueExpression LiteralFloatingPointValue:
                Result = BuildLiteralFloatingPointValueExpression(LiteralFloatingPointValue, out IExprSet<IArithExprCapsule> LiteralFloatingPointValueExpr);
                ResultExpr = LiteralFloatingPointValueExpr;
                break;
            case LiteralNullExpression:
                Result = BuildLiteralNullExpression(out IExprSet<IRefExprCapsule> LiteralNullExpr);
                ResultExpr = LiteralNullExpr;
                break;
            case NewObjectExpression NewObject:
                Result = BuildNewObjectExpression(verificationContext, NewObject, out IExprSet<IExprCapsule> NewObjectExpr);
                ResultExpr = NewObjectExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = ResultExpr!;
        return Result;
    }

    private bool BuildLiteralBooleanValueExpression(LiteralBooleanValueExpression literalBooleanValueExpression, out IExprSet<IBoolExprCapsule> resultExpr)
    {
        IBoolExprCapsule Expr = Context.CreateBooleanValue(literalBooleanValueExpression.Value);
        resultExpr = Expr.ToSingleSet();
        return true;
    }

    private bool BuildLiteralIntegerValueExpression(LiteralIntegerValueExpression literalIntegerValueExpression, out IExprSet<IIntExprCapsule> resultExpr)
    {
        IIntExprCapsule Expr = Context.CreateIntegerValue(literalIntegerValueExpression.Value);
        resultExpr = Expr.ToSingleSet();
        return true;
    }

    private bool BuildLiteralFloatingPointValueExpression(LiteralFloatingPointValueExpression literalFloatingPointValueExpression, out IExprSet<IArithExprCapsule> resultExpr)
    {
        IArithExprCapsule Expr = Context.CreateFloatingPointValue(literalFloatingPointValueExpression.Value);
        resultExpr = Expr.ToSingleSet();
        return true;
    }

    private bool BuildLiteralNullExpression(out IExprSet<IRefExprCapsule> resultExpr)
    {
        resultExpr = Context.NullSet;
        return true;
    }

    private bool BuildNewObjectExpression(VerificationContext verificationContext, NewObjectExpression newObjectExpression, out IExprSet<IExprCapsule> resultExpr)
    {
        bool Result = true;
        resultExpr = verificationContext.ObjectManager.CreateObjectInitializer(newObjectExpression.ObjectType);

        return Result;
    }

    private static int TemporaryResultIndex;
}
