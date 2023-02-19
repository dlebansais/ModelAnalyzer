namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a code verifier.
/// </summary>
internal partial class Verifier : IDisposable
{
    private bool BuildExpression<T>(VerificationContext verificationContext, IExpression expression, out IExprBase<T, T> expr)
        where T : IExprCapsule
    {
        bool Result = false;
        IExprBase<IExprCapsule, IExprCapsule>? ResultExpr = null;

        switch (expression)
        {
            case VariableValueExpression VariableValue:
                Result = BuildVariableValueExpression(verificationContext, VariableValue, out IExprBase<IExprCapsule, IExprCapsule> VariableValueExpr);
                ResultExpr = VariableValueExpr;
                break;
            case PrivateFunctionCallExpression PrivateFunctionCall:
                Result = BuildPrivateFunctionCallExpression(verificationContext, PrivateFunctionCall, out IExprBase<IExprCapsule, IExprCapsule> PrivateFunctionCallExpr);
                ResultExpr = PrivateFunctionCallExpr;
                break;
            case PublicFunctionCallExpression PublicFunctionCall:
                Result = BuildPublicFunctionCallExpression(verificationContext, PublicFunctionCall, out IExprBase<IExprCapsule, IExprCapsule> PublicFunctionCallExpr);
                ResultExpr = PublicFunctionCallExpr;
                break;
            case IBinaryExpression Binary:
                Result = BuildBinaryExpression(verificationContext, Binary, out IExprBase<IExprCapsule, IExprCapsule> BinaryExpr);
                ResultExpr = BinaryExpr;
                break;
            case IUnaryExpression Unary:
                Result = BuildUnaryExpression(verificationContext, Unary, out IExprBase<IExprCapsule, IExprCapsule> unaryExpr);
                ResultExpr = unaryExpr;
                break;
            case ILiteralExpression Literal:
                Result = BuildLiteralValueExpression(verificationContext, Literal, out IExprBase<IExprCapsule, IExprCapsule> LiteralExpr);
                ResultExpr = LiteralExpr;
                break;
            case ElementAccessExpression ElementAccess:
                Result = BuildElementAccessExpression(verificationContext, ElementAccess, out IExprBase<IExprCapsule, IExprCapsule> ElementAccessExpr);
                ResultExpr = ElementAccessExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = (IExprBase<T, T>)ResultExpr!;
        return Result;
    }

    private bool BuildVariableValueExpression(VerificationContext verificationContext, VariableValueExpression variableValueExpression, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
    {
        return BuildVariableValueExpression(verificationContext, variableValueExpression.VariablePath, out resultExpr);
    }

    private bool BuildVariableValueExpression(VerificationContext verificationContext, List<IVariable> variablePath, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
    {
        return BuildVariableValueExpression(verificationContext, variablePath, pathIndex: 0, out resultExpr);
    }

    private bool BuildVariableValueExpression(VerificationContext verificationContext, List<IVariable> variablePath, int pathIndex, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
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

        resultExpr = verificationContext.ObjectManager.CreateValueExpr(HostMethod is null ? verificationContext.Instance : null, HostMethod, VariableName!, VariableType!);

        if (pathIndex + 1 < variablePath.Count)
        {
            if (pathIndex + 2 == variablePath.Count && VariableType!.IsArray)
            {
                Debug.Assert(variablePath[pathIndex + 1].Name.Text == "Length");
                Debug.Assert(resultExpr is ExprArray<IArrayExprCapsule>);

                ExprArray<IArrayExprCapsule> ArrayExpr = (ExprArray<IArrayExprCapsule>)resultExpr;
                resultExpr = ArrayExpr.SizeExpression.ToSingleSet();
                return true;
            }
            else
            {
                ClassModel ClassModel = verificationContext.ObjectManager.TypeToModel(VariableType!);

                Debug.Assert(resultExpr.MainExpression is IObjectRefExprCapsule);
                IObjectRefExprCapsule ReferenceExpr = (IObjectRefExprCapsule)resultExpr.MainExpression;
                Instance Reference = new() { ClassModel = ClassModel, Expr = ReferenceExpr };

                VerificationContext NewVerificationContext = verificationContext with
                {
                    Instance = Reference,
                    HostMethod = null,
                    HostBlock = null,
                    ResultLocal = null,
                };

                return BuildVariableValueExpression(NewVerificationContext, variablePath, pathIndex + 1, out resultExpr);
            }
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
        if (verificationContext.HostBlock is null)
            return;

        ReadOnlyLocalTable LocalTable = verificationContext.HostBlock.LocalTable;

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

    private bool BuildPrivateFunctionCallExpression(VerificationContext verificationContext, PrivateFunctionCallExpression functionCallExpression, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
    {
        bool Result = false;
        resultExpr = null!;

        foreach (var Entry in MethodTable)
            if (Entry.Key == functionCallExpression.MethodName)
            {
                Method CalledFunction = Entry.Value;
                Result = BuildPrivateFunctionCallExpression(verificationContext, functionCallExpression, CalledFunction, out resultExpr);
                break;
            }

        Debug.Assert(resultExpr is not null);

        return Result;
    }

    private bool BuildPrivateFunctionCallExpression(VerificationContext verificationContext, PrivateFunctionCallExpression functionCallExpression, Method calledFunction, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
    {
        resultExpr = verificationContext.ObjectManager.GetDefaultExpr(calledFunction.ReturnType);
        List<Argument> ArgumentList = functionCallExpression.ArgumentList;

        int Index = 0;
        foreach (var Entry in calledFunction.ParameterTable)
        {
            Argument Argument = ArgumentList[Index++];
            Parameter Parameter = Entry.Value;

            if (!BuildExpression(verificationContext, Argument.Expression, out IExprBase<IExprCapsule, IExprCapsule> InitializerExpr))
                return false;

            verificationContext.ObjectManager.CreateVariable(owner: null, calledFunction, Parameter.Name, Parameter.Type, verificationContext.Branch, InitializerExpr);
        }

        LocalName CallResultName = new LocalName() { Text = CreateTemporaryResultLocal() };
        Local CallResult = new Local() { Name = CallResultName, Type = calledFunction.ReturnType, Initializer = null };
        verificationContext.ObjectManager.CreateVariable(owner: null, calledFunction, CallResult.Name, CallResult.Type, branch: null, initializerExpr: null);

        VerificationContext CallVerificationContext = verificationContext with { HostMethod = calledFunction, HostBlock = calledFunction.RootBlock, ResultLocal = CallResult };

        if (!AddMethodRequires(CallVerificationContext, functionCallExpression.CallerClassName, functionCallExpression.LocationId, checkOpposite: true))
            return false;

        if (!AddStatementListExecution(CallVerificationContext, calledFunction.RootBlock))
            return false;

        if (!AddMethodEnsures(CallVerificationContext, keepNormal: true))
            return false;

        resultExpr = verificationContext.ObjectManager.CreateValueExpr(owner: null, calledFunction, CallResult.Name, CallResult.Type);

        return true;
    }

    private bool BuildPublicFunctionCallExpression(VerificationContext verificationContext, PublicFunctionCallExpression functionCallExpression, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
    {
        bool Result = false;
        resultExpr = null!;

        ClassModel ClassModel;
        Instance CalledInstance;

        if (functionCallExpression.IsStatic)
        {
            ClassModel = GetClassModel(verificationContext, functionCallExpression.ClassName);
            CalledInstance = new() { ClassModel = ClassModel, Expr = verificationContext.ObjectManager.Context.Null };
        }
        else
        {
            ClassModel = GetLastClassModel(verificationContext, functionCallExpression.VariablePath);

            BuildVariableValueExpression(verificationContext, functionCallExpression.VariablePath, out IExprBase<IExprCapsule, IExprCapsule> CalledClassExpr);

            Debug.Assert(CalledClassExpr.MainExpression is IObjectRefExprCapsule);
            IObjectRefExprCapsule CalledInstanceExpr = (IObjectRefExprCapsule)CalledClassExpr.MainExpression;
            CalledInstance = new() { ClassModel = ClassModel, Expr = CalledInstanceExpr };
        }

        foreach (var Entry in ClassModel.MethodTable)
            if (Entry.Key == functionCallExpression.MethodName)
            {
                Method CalledFunction = Entry.Value;
                Result = BuildPublicFunctionCallExpression(verificationContext, functionCallExpression, CalledInstance, CalledFunction, out resultExpr);
                break;
            }

        Debug.Assert(resultExpr is not null);

        return Result;
    }

    private bool BuildPublicFunctionCallExpression(VerificationContext verificationContext, PublicFunctionCallExpression functionCallExpression, Instance calledInstance, Method calledFunction, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
    {
        resultExpr = verificationContext.ObjectManager.GetDefaultExpr(calledFunction.ReturnType);
        List<Argument> ArgumentList = functionCallExpression.ArgumentList;

        int Index = 0;
        foreach (var Entry in calledFunction.ParameterTable)
        {
            Argument Argument = ArgumentList[Index++];
            Parameter Parameter = Entry.Value;

            if (!BuildExpression(verificationContext, Argument.Expression, out IExprBase<IExprCapsule, IExprCapsule> InitializerExpr))
                return false;

            verificationContext.ObjectManager.CreateVariable(owner: null, calledFunction, Parameter.Name, Parameter.Type, verificationContext.Branch, InitializerExpr);
        }

        LocalName CallResultName = new LocalName() { Text = CreateTemporaryResultLocal() };
        Local CallResult = new Local() { Name = CallResultName, Type = calledFunction.ReturnType, Initializer = null };
        verificationContext.ObjectManager.CreateVariable(owner: null, calledFunction, CallResult.Name, CallResult.Type, branch: null, initializerExpr: null);

        VerificationContext CallVerificationContext = verificationContext with
        {
            Instance = calledInstance,
            HostMethod = calledFunction,
            HostBlock = calledFunction.RootBlock,
            ResultLocal = CallResult,
        };

        bool IsStaticCall = calledInstance.Expr == verificationContext.ObjectManager.Context.Null;

        if (!AddMethodRequires(CallVerificationContext, functionCallExpression.CallerClassName, functionCallExpression.LocationId, checkOpposite: true))
            return false;

        if (!IsStaticCall)
            verificationContext.ObjectManager.ClearState(calledInstance);

        if (!AddMethodEnsures(CallVerificationContext, keepNormal: true))
            return false;

        if (!IsStaticCall)
            if (!AddClassInvariant(CallVerificationContext))
                return false;

        resultExpr = verificationContext.ObjectManager.CreateValueExpr(owner: null, calledFunction, CallResult.Name, CallResult.Type);

        return true;
    }

    private string CreateTemporaryResultLocal()
    {
        string Result = $"temp_{TemporaryResultIndex}";
        TemporaryResultIndex++;

        return Result;
    }

    private bool BuildBinaryExpression(VerificationContext verificationContext, IBinaryExpression binaryExpression, out IExprBase<IExprCapsule, IExprCapsule> expr)
    {
        bool Result = false;
        IExprBase<IExprCapsule, IExprCapsule>? ResultExpr = null;

        switch (binaryExpression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                Result = BuildBinaryArithmeticExpression(verificationContext, BinaryArithmetic, out IExprBase<IArithExprCapsule, IArithExprCapsule> BinaryArithmeticExpr);
                ResultExpr = BinaryArithmeticExpr;
                break;
            case RemainderExpression Remainder:
                Result = BuildRemainderExpression(verificationContext, Remainder, out IExprBase<IIntExprCapsule, IIntExprCapsule> RemainderExpr);
                ResultExpr = RemainderExpr;
                break;
            case BinaryLogicalExpression BinaryLogical:
                Result = BuildBinaryLogicalExpression(verificationContext, BinaryLogical, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> BinaryLogicalExpr);
                ResultExpr = BinaryLogicalExpr;
                break;
            case EqualityExpression Equality:
                Result = BuildEqualityExpression(verificationContext, Equality, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> EqualityExpr);
                ResultExpr = EqualityExpr;
                break;
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(verificationContext, Comparison, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> ComparisonExpr);
                ResultExpr = ComparisonExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = ResultExpr!;
        return Result;
    }

    private bool BuildBinaryArithmeticExpression(VerificationContext verificationContext, BinaryArithmeticExpression binaryArithmeticExpression, out IExprBase<IArithExprCapsule, IArithExprCapsule> resultExprSet)
    {
        bool ResultLeft = BuildExpression(verificationContext, binaryArithmeticExpression.Left, out IExprBase<IArithExprCapsule, IArithExprCapsule> Left);
        bool ResultRight = BuildExpression(verificationContext, binaryArithmeticExpression.Right, out IExprBase<IArithExprCapsule, IArithExprCapsule> Right);

        Debug.Assert(Left is IExprSingle<IArithExprCapsule>);
        Debug.Assert(Right is IExprSingle<IArithExprCapsule>);

        IExprSingle<IArithExprCapsule> LeftSingle = (IExprSingle<IArithExprCapsule>)Left;
        IExprSingle<IArithExprCapsule> RightSingle = (IExprSingle<IArithExprCapsule>)Right;

        if (binaryArithmeticExpression.Operator == BinaryArithmeticOperator.Divide)
        {
            IExprSingle<IBoolExprCapsule> AssertionExpr = Context.CreateNotEqualExprSet(RightSingle, Context.ZeroSet);

            if (!AddMethodAssertionOpposite(verificationContext, AssertionExpr, ClassName.Empty, binaryArithmeticExpression.LocationId, binaryArithmeticExpression.ToString(), VerificationErrorType.AssumeError))
            {
                resultExprSet = Context.ZeroSet;
                return false;
            }
        }

        IArithExprCapsule ResultExpr = OperatorBuilder.BinaryArithmetic[binaryArithmeticExpression.Operator](Context, LeftSingle.MainExpression, RightSingle.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultLeft && ResultRight;
    }

    private bool BuildRemainderExpression(VerificationContext verificationContext, RemainderExpression remainderExpression, out IExprBase<IIntExprCapsule, IIntExprCapsule> resultExprSet)
    {
        bool ResultLeft = BuildExpression(verificationContext, remainderExpression.Left, out IExprBase<IIntExprCapsule, IIntExprCapsule> Left);
        bool ResultRight = BuildExpression(verificationContext, remainderExpression.Right, out IExprBase<IIntExprCapsule, IIntExprCapsule> Right);

        Debug.Assert(Left is IExprSingle<IIntExprCapsule>);
        Debug.Assert(Right is IExprSingle<IIntExprCapsule>);

        IExprSingle<IIntExprCapsule> LeftSingle = (IExprSingle<IIntExprCapsule>)Left;
        IExprSingle<IIntExprCapsule> RightSingle = (IExprSingle<IIntExprCapsule>)Right;

        IExprSingle<IBoolExprCapsule> AssertionExpr = Context.CreateNotEqualExprSet(RightSingle, Context.ZeroSet);

        if (!AddMethodAssertionOpposite(verificationContext, AssertionExpr, ClassName.Empty, remainderExpression.LocationId, remainderExpression.ToString(), VerificationErrorType.AssumeError))
        {
            resultExprSet = Context.ZeroSet;
            return false;
        }

        IIntExprCapsule ResultExpr = Context.CreateRemainderExpr(LeftSingle.MainExpression, RightSingle.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultLeft && ResultRight;
    }

    private bool BuildBinaryLogicalExpression(VerificationContext verificationContext, BinaryLogicalExpression binaryLogicalExpression, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> resultExprSet)
    {
        bool ResultLeft = BuildExpression(verificationContext, binaryLogicalExpression.Left, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> Left);
        bool ResultRight = BuildExpression(verificationContext, binaryLogicalExpression.Right, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> Right);

        Debug.Assert(Left is IExprSingle<IBoolExprCapsule>);
        Debug.Assert(Right is IExprSingle<IBoolExprCapsule>);

        IBoolExprCapsule ResultExpr = OperatorBuilder.BinaryLogical[binaryLogicalExpression.Operator](Context, Left.MainExpression, Right.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultLeft && ResultRight;
    }

    private bool BuildEqualityExpression(VerificationContext verificationContext, EqualityExpression equalityExpression, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> resultExprSet)
    {
        bool ResultLeft = BuildExpression(verificationContext, equalityExpression.Left, out IExprBase<IExprCapsule, IExprCapsule> Left);
        bool ResultRight = BuildExpression(verificationContext, equalityExpression.Right, out IExprBase<IExprCapsule, IExprCapsule> Right);

        IBoolExprCapsule ResultExpr = OperatorBuilder.Equality[equalityExpression.Operator](Context, Left.MainExpression, Right.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultLeft && ResultRight;
    }

    private bool BuildComparisonExpression(VerificationContext verificationContext, ComparisonExpression comparisonExpression, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> resultExprSet)
    {
        bool ResultLeft = BuildExpression(verificationContext, comparisonExpression.Left, out IExprBase<IArithExprCapsule, IArithExprCapsule> Left);
        bool ResultRight = BuildExpression(verificationContext, comparisonExpression.Right, out IExprBase<IArithExprCapsule, IArithExprCapsule> Right);

        Debug.Assert(Left is IExprSingle<IArithExprCapsule>);
        Debug.Assert(Right is IExprSingle<IArithExprCapsule>);

        IBoolExprCapsule ResultExpr = OperatorBuilder.Comparison[comparisonExpression.Operator](Context, Left.MainExpression, Right.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultLeft && ResultRight;
    }

    private bool BuildUnaryExpression(VerificationContext verificationContext, IUnaryExpression unaryExpression, out IExprBase<IExprCapsule, IExprCapsule> expr)
    {
        bool Result = false;
        IExprBase<IExprCapsule, IExprCapsule>? ResultExpr = null;

        switch (unaryExpression)
        {
            case UnaryArithmeticExpression UnaryArithmetic:
                Result = BuildUnaryArithmeticExpression(verificationContext, UnaryArithmetic, out IExprBase<IArithExprCapsule, IArithExprCapsule> UnaryArithmeticExpr);
                ResultExpr = UnaryArithmeticExpr;
                break;
            case UnaryLogicalExpression UnaryLogical:
                Result = BuildUnaryLogicalExpression(verificationContext, UnaryLogical, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> UnaryLogicalExpr);
                ResultExpr = UnaryLogicalExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = ResultExpr!;
        return Result;
    }

    private bool BuildUnaryArithmeticExpression(VerificationContext verificationContext, UnaryArithmeticExpression unaryArithmeticExpression, out IExprBase<IArithExprCapsule, IArithExprCapsule> resultExprSet)
    {
        bool ResultOperand = BuildExpression(verificationContext, unaryArithmeticExpression.Operand, out IExprBase<IArithExprCapsule, IArithExprCapsule> Operand);

        Debug.Assert(Operand is IExprSingle<IArithExprCapsule>);

        IArithExprCapsule ResultExpr = OperatorBuilder.UnaryArithmetic[unaryArithmeticExpression.Operator](Context, Operand.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultOperand;
    }

    private bool BuildUnaryLogicalExpression(VerificationContext verificationContext, UnaryLogicalExpression unaryLogicalExpression, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> resultExprSet)
    {
        bool ResultOperand = BuildExpression(verificationContext, unaryLogicalExpression.Operand, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> Operand);

        Debug.Assert(Operand is IExprSingle<IBoolExprCapsule>);

        IBoolExprCapsule ResultExpr = OperatorBuilder.UnaryLogical[unaryLogicalExpression.Operator](Context, Operand.MainExpression);
        resultExprSet = ResultExpr.ToSingleSet();

        return ResultOperand;
    }

    private bool BuildLiteralValueExpression(VerificationContext verificationContext, ILiteralExpression literalExpression, out IExprBase<IExprCapsule, IExprCapsule> expr)
    {
        bool Result = false;
        IExprBase<IExprCapsule, IExprCapsule>? ResultExpr = null;

        switch (literalExpression)
        {
            case LiteralBooleanValueExpression LiteralBooleanValue:
                Result = BuildLiteralBooleanValueExpression(LiteralBooleanValue, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> LiteralBooleanValueExpr);
                ResultExpr = LiteralBooleanValueExpr;
                break;
            case LiteralIntegerValueExpression LiteralIntegerValue:
                Result = BuildLiteralIntegerValueExpression(LiteralIntegerValue, out IExprBase<IIntExprCapsule, IIntExprCapsule> LiteralIntegerValueExpr);
                ResultExpr = LiteralIntegerValueExpr;
                break;
            case LiteralFloatingPointValueExpression LiteralFloatingPointValue:
                Result = BuildLiteralFloatingPointValueExpression(LiteralFloatingPointValue, out IExprBase<IArithExprCapsule, IArithExprCapsule> LiteralFloatingPointValueExpr);
                ResultExpr = LiteralFloatingPointValueExpr;
                break;
            case LiteralNullExpression:
                Result = BuildLiteralNullExpression(out IExprBase<IRefExprCapsule, IRefExprCapsule> LiteralNullExpr);
                ResultExpr = LiteralNullExpr;
                break;
            case NewObjectExpression NewObject:
                Result = BuildNewObjectExpression(verificationContext, NewObject, out IExprBase<IExprCapsule, IExprCapsule> NewObjectExpr);
                ResultExpr = NewObjectExpr;
                break;
            case NewArrayExpression NewArray:
                Result = BuildNewArrayExpression(verificationContext, NewArray, out IExprBase<IExprCapsule, IExprCapsule> NewArrayExpr);
                ResultExpr = NewArrayExpr;
                break;
        }

        Debug.Assert(ResultExpr is not null);

        expr = ResultExpr!;
        return Result;
    }

    private bool BuildLiteralBooleanValueExpression(LiteralBooleanValueExpression literalBooleanValueExpression, out IExprBase<IBoolExprCapsule, IBoolExprCapsule> resultExpr)
    {
        IBoolExprCapsule Expr = Context.CreateBooleanValue(literalBooleanValueExpression.Value);
        resultExpr = Expr.ToSingleSet();
        return true;
    }

    private bool BuildLiteralIntegerValueExpression(LiteralIntegerValueExpression literalIntegerValueExpression, out IExprBase<IIntExprCapsule, IIntExprCapsule> resultExpr)
    {
        IIntExprCapsule Expr = Context.CreateIntegerValue(literalIntegerValueExpression.Value);
        resultExpr = Expr.ToSingleSet();
        return true;
    }

    private bool BuildLiteralFloatingPointValueExpression(LiteralFloatingPointValueExpression literalFloatingPointValueExpression, out IExprBase<IArithExprCapsule, IArithExprCapsule> resultExpr)
    {
        IArithExprCapsule Expr = Context.CreateFloatingPointValue(literalFloatingPointValueExpression.Value);
        resultExpr = Expr.ToSingleSet();
        return true;
    }

    private bool BuildLiteralNullExpression(out IExprBase<IRefExprCapsule, IRefExprCapsule> resultExpr)
    {
        resultExpr = Context.NullSet;
        return true;
    }

    private bool BuildNewObjectExpression(VerificationContext verificationContext, NewObjectExpression newObjectExpression, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
    {
        bool Result = true;
        resultExpr = verificationContext.ObjectManager.CreateObjectInitializer(newObjectExpression.ObjectType);

        return Result;
    }

    private bool BuildNewArrayExpression(VerificationContext verificationContext, NewArrayExpression newArrayExpression, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
    {
        bool Result = true;
        resultExpr = verificationContext.ObjectManager.CreateArrayInitializer(newArrayExpression.ArrayType, newArrayExpression.ArraySize);

        return Result;
    }

    private bool BuildElementAccessExpression(VerificationContext verificationContext, ElementAccessExpression elementAccessExpression, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
    {
        return BuildElementAccessExpression(verificationContext, elementAccessExpression.VariablePath, elementAccessExpression.ElementIndex, out resultExpr);
    }

    private bool BuildElementAccessExpression(VerificationContext verificationContext, List<IVariable> variablePath, Expression elementIndex, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
    {
        return BuildElementAccessExpression(verificationContext, variablePath, pathIndex: 0, elementIndex, out resultExpr);
    }

    private bool BuildElementAccessExpression(VerificationContext verificationContext, List<IVariable> variablePath, int pathIndex, Expression elementIndex, out IExprBase<IExprCapsule, IExprCapsule> resultExpr)
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

        resultExpr = verificationContext.ObjectManager.CreateValueExpr(HostMethod is null ? verificationContext.Instance : null, HostMethod, VariableName!, VariableType!);

        if (pathIndex + 1 < variablePath.Count)
        {
            ClassModel ClassModel = verificationContext.ObjectManager.TypeToModel(VariableType!);

            Debug.Assert(resultExpr.MainExpression is IObjectRefExprCapsule);
            IObjectRefExprCapsule ObjectReferenceExpr = (IObjectRefExprCapsule)resultExpr.MainExpression;
            Instance Reference = new() { ClassModel = ClassModel, Expr = ObjectReferenceExpr };

            VerificationContext NewVerificationContext = verificationContext with
            {
                Instance = Reference,
                HostMethod = null,
                HostBlock = null,
                ResultLocal = null,
            };

            return BuildElementAccessExpression(NewVerificationContext, variablePath, pathIndex + 1, elementIndex, out resultExpr);
        }
        else
        {
            Debug.Assert(resultExpr is ExprArray<IArrayExprCapsule>);

            ExprArray<IArrayExprCapsule> ArrayResultExpr = (ExprArray<IArrayExprCapsule>)resultExpr;
            IArrayExprCapsule ArrayExpr = ArrayResultExpr.ArrayExpression;

            // Index is either an int literal or a single variable. This can't possibly fail.
            bool IndexBuildSuccess = BuildExpression(verificationContext, elementIndex, out IExprBase<IIntExprCapsule, IIntExprCapsule> IndexExpr);
            Debug.Assert(IndexBuildSuccess);

            IExprCapsule ElementExpr = Context.CreateGetElementExpr(ArrayExpr, IndexExpr.MainExpression);

            Debug.Assert(ElementExpr is ISimpleExprCapsule);

            resultExpr = ((ISimpleExprCapsule)ElementExpr).ToSingle();

            return true;
        }
    }

    private static int TemporaryResultIndex;
}
