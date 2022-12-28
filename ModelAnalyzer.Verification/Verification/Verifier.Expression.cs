namespace ModelAnalyzer;

using System;
using System.Diagnostics;
using Microsoft.Z3;

/// <summary>
/// Represents a code verifier.
/// </summary>
internal partial class Verifier : IDisposable
{
    private T BuildExpression<T>(AliasTable aliasTable, ReadOnlyParameterTable parameterTable, Field? resultField, IExpression expression)
        where T : Expr
    {
        Expr Result = null!;
        bool IsAssigned = false;

        switch (expression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                Result = BuildBinaryArithmeticExpression(aliasTable, parameterTable, resultField, BinaryArithmetic);
                IsAssigned = true;
                break;
            case UnaryArithmeticExpression UnaryArithmetic:
                Result = BuildUnaryArithmeticExpression(aliasTable, parameterTable, resultField, UnaryArithmetic);
                IsAssigned = true;
                break;
            case BinaryLogicalExpression BinaryLogical:
                Result = BuildBinaryLogicalExpression(aliasTable, parameterTable, resultField, BinaryLogical);
                IsAssigned = true;
                break;
            case UnaryLogicalExpression UnaryLogical:
                Result = BuildUnaryLogicalExpression(aliasTable, parameterTable, resultField, UnaryLogical);
                IsAssigned = true;
                break;
            case EqualityExpression Equality:
                Result = BuildEqualityExpression(aliasTable, parameterTable, resultField, Equality);
                IsAssigned = true;
                break;
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(aliasTable, parameterTable, resultField, Comparison);
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
                Result = BuildVariableValueExpression(aliasTable, parameterTable, resultField, VariableValue);
                IsAssigned = true;
                break;
        }

        Debug.Assert(IsAssigned);

        return (T)Result;
    }

    private ArithExpr BuildBinaryArithmeticExpression(AliasTable aliasTable, ReadOnlyParameterTable parameterTable, Field? resultField, BinaryArithmeticExpression binaryArithmeticExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, parameterTable, resultField, binaryArithmeticExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, parameterTable, resultField, binaryArithmeticExpression.Right);
        return OperatorBuilder.BinaryArithmetic[binaryArithmeticExpression.Operator](Context, Left, Right);
    }

    private ArithExpr BuildUnaryArithmeticExpression(AliasTable aliasTable, ReadOnlyParameterTable parameterTable, Field? resultField, UnaryArithmeticExpression unaryArithmeticExpression)
    {
        ArithExpr Operand = BuildExpression<ArithExpr>(aliasTable, parameterTable, resultField, unaryArithmeticExpression.Operand);
        return OperatorBuilder.UnaryArithmetic[unaryArithmeticExpression.Operator](Context, Operand);
    }

    private BoolExpr BuildBinaryLogicalExpression(AliasTable aliasTable, ReadOnlyParameterTable parameterTable, Field? resultField, BinaryLogicalExpression binaryLogicalExpression)
    {
        BoolExpr Left = BuildExpression<BoolExpr>(aliasTable, parameterTable, resultField, binaryLogicalExpression.Left);
        BoolExpr Right = BuildExpression<BoolExpr>(aliasTable, parameterTable, resultField, binaryLogicalExpression.Right);
        return OperatorBuilder.BinaryLogical[binaryLogicalExpression.Operator](Context, Left, Right);
    }

    private BoolExpr BuildUnaryLogicalExpression(AliasTable aliasTable, ReadOnlyParameterTable parameterTable, Field? resultField, UnaryLogicalExpression unaryLogicalExpression)
    {
        BoolExpr Operand = BuildExpression<BoolExpr>(aliasTable, parameterTable, resultField, unaryLogicalExpression.Operand);
        return OperatorBuilder.UnaryLogical[unaryLogicalExpression.Operator](Context, Operand);
    }

    private BoolExpr BuildEqualityExpression(AliasTable aliasTable, ReadOnlyParameterTable parameterTable, Field? resultField, EqualityExpression equalityExpression)
    {
        Expr Left = BuildExpression<Expr>(aliasTable, parameterTable, resultField, equalityExpression.Left);
        Expr Right = BuildExpression<Expr>(aliasTable, parameterTable, resultField, equalityExpression.Right);
        return OperatorBuilder.Equality[equalityExpression.Operator](Context, Left, Right);
    }

    private BoolExpr BuildComparisonExpression(AliasTable aliasTable, ReadOnlyParameterTable parameterTable, Field? resultField, ComparisonExpression comparisonExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, parameterTable, resultField, comparisonExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, parameterTable, resultField, comparisonExpression.Right);
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

    private Expr BuildVariableValueExpression(AliasTable aliasTable, ReadOnlyParameterTable parameterTable, Field? resultField, VariableValueExpression variableValueExpression)
    {
        IVariable Variable = variableValueExpression.GetVariable(FieldTable, parameterTable, resultField);

        string VariableString;

        if (Variable == resultField)
            VariableString = resultField.Name.Text;
        else
        {
            VariableAlias VariableAliasName = aliasTable.GetAlias(Variable);
            VariableString = VariableAliasName.ToString();
        }

        return CreateVariableExpr(VariableString, Variable.Type);
    }
}
