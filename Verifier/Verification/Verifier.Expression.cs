namespace ModelAnalyzer;

using System;
using System.Diagnostics;
using Microsoft.Z3;

/// <summary>
/// Represents a code verifier.
/// </summary>
internal partial class Verifier : IDisposable
{
    private T BuildExpression<T>(AliasTable aliasTable, IExpression expression)
        where T : Expr
    {
        Expr Result = null!;
        bool IsAssigned = false;

        switch (expression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                Result = BuildBinaryArithmeticExpression(aliasTable, BinaryArithmetic);
                IsAssigned = true;
                break;
            case UnaryArithmeticExpression UnaryArithmetic:
                Result = BuildUnaryArithmeticExpression(aliasTable, UnaryArithmetic);
                IsAssigned = true;
                break;
            case BinaryLogicalExpression BinaryLogical:
                Result = BuildBinaryLogicalExpression(aliasTable, BinaryLogical);
                IsAssigned = true;
                break;
            case UnaryLogicalExpression UnaryLogical:
                Result = BuildUnaryLogicalExpression(aliasTable, UnaryLogical);
                IsAssigned = true;
                break;
            case EqualityExpression Equality:
                Result = BuildEqualityExpression(aliasTable, Equality);
                IsAssigned = true;
                break;
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(aliasTable, Comparison);
                IsAssigned = true;
                break;
            case LiteralBooleanValueExpression LiteralBooleanValue:
                Result = BuildLiteralBooleanValueExpression(LiteralBooleanValue);
                IsAssigned = true;
                break;
            case LiteralIntegerValueExpression LiteralIntegerValue:
                Result = BuildLiteralIntegerValueExpression(LiteralIntegerValue);
                IsAssigned = true;
                break;
            case LiteralFloatingPointValueExpression LiteralFloatingPointValue:
                Result = BuildLiteralFloatingPointValueExpression(LiteralFloatingPointValue);
                IsAssigned = true;
                break;
            case VariableValueExpression VariableValue:
                Result = BuildVariableValueExpression(aliasTable, VariableValue);
                IsAssigned = true;
                break;
        }

        Debug.Assert(IsAssigned);

        return (T)Result;
    }

    private ArithExpr BuildBinaryArithmeticExpression(AliasTable aliasTable, BinaryArithmeticExpression binaryArithmeticExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, binaryArithmeticExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, binaryArithmeticExpression.Right);
        return OperatorBuilder.BinaryArithmetic[binaryArithmeticExpression.Operator](Context, Left, Right);
    }

    private ArithExpr BuildUnaryArithmeticExpression(AliasTable aliasTable, UnaryArithmeticExpression unaryArithmeticExpression)
    {
        ArithExpr Operand = BuildExpression<ArithExpr>(aliasTable, unaryArithmeticExpression.Operand);
        return OperatorBuilder.UnaryArithmetic[unaryArithmeticExpression.Operator](Context, Operand);
    }

    private BoolExpr BuildBinaryLogicalExpression(AliasTable aliasTable, BinaryLogicalExpression binaryLogicalExpression)
    {
        BoolExpr Left = BuildExpression<BoolExpr>(aliasTable, binaryLogicalExpression.Left);
        BoolExpr Right = BuildExpression<BoolExpr>(aliasTable, binaryLogicalExpression.Right);
        return OperatorBuilder.BinaryLogical[binaryLogicalExpression.Operator](Context, Left, Right);
    }

    private BoolExpr BuildUnaryLogicalExpression(AliasTable aliasTable, UnaryLogicalExpression unaryLogicalExpression)
    {
        BoolExpr Operand = BuildExpression<BoolExpr>(aliasTable, unaryLogicalExpression.Operand);
        return OperatorBuilder.UnaryLogical[unaryLogicalExpression.Operator](Context, Operand);
    }

    private BoolExpr BuildEqualityExpression(AliasTable aliasTable, EqualityExpression equalityExpression)
    {
        Expr Left = BuildExpression<Expr>(aliasTable, equalityExpression.Left);
        Expr Right = BuildExpression<Expr>(aliasTable, equalityExpression.Right);
        return OperatorBuilder.Equality[equalityExpression.Operator](Context, Left, Right);
    }

    private BoolExpr BuildComparisonExpression(AliasTable aliasTable, ComparisonExpression comparisonExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, comparisonExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, comparisonExpression.Right);
        return OperatorBuilder.Comparison[comparisonExpression.Operator](Context, Left, Right);
    }

    private BoolExpr BuildLiteralBooleanValueExpression(LiteralBooleanValueExpression literalBooleanValueExpression)
    {
        return CreateBooleanExpr(literalBooleanValueExpression.Value);
    }

    private ArithExpr BuildLiteralIntegerValueExpression(LiteralIntegerValueExpression literalIntegerValueExpression)
    {
        return CreateIntegerExpr(literalIntegerValueExpression.Value);
    }

    private ArithExpr BuildLiteralFloatingPointValueExpression(LiteralFloatingPointValueExpression literalFloatingPointValueExpression)
    {
        return CreateFloatingPointExpr(literalFloatingPointValueExpression.Value);
    }

    private Expr BuildVariableValueExpression(AliasTable aliasTable, VariableValueExpression variableValueExpression)
    {
        IVariable Variable = variableValueExpression.Variable;
        string VariableName = Variable.Name;
        string VariableAliasName = aliasTable.GetAlias(VariableName);

        Expr Result = Zero;
        bool IsHandled = false;

        switch (Variable.VariableType)
        {
            case ExpressionType.Boolean:
                Result = Context.MkBoolConst(VariableAliasName);
                IsHandled = true;
                break;

            case ExpressionType.Integer:
                Result = Context.MkIntConst(VariableAliasName);
                IsHandled = true;
                break;

            case ExpressionType.FloatingPoint:
                Result = Context.MkRealConst(VariableAliasName);
                IsHandled = true;
                break;
        }

        Debug.Assert(IsHandled);

        return Result;
    }
}
