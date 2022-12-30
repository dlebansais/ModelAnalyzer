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
    private T BuildExpression<T>(AliasTable aliasTable, Method? hostMethod, Field? resultField, IExpression expression)
        where T : Expr
    {
        Expr Result = null!;
        bool IsAssigned = false;

        switch (expression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                Result = BuildBinaryArithmeticExpression(aliasTable, hostMethod, resultField, BinaryArithmetic);
                IsAssigned = true;
                break;
            case UnaryArithmeticExpression UnaryArithmetic:
                Result = BuildUnaryArithmeticExpression(aliasTable, hostMethod, resultField, UnaryArithmetic);
                IsAssigned = true;
                break;
            case BinaryLogicalExpression BinaryLogical:
                Result = BuildBinaryLogicalExpression(aliasTable, hostMethod, resultField, BinaryLogical);
                IsAssigned = true;
                break;
            case UnaryLogicalExpression UnaryLogical:
                Result = BuildUnaryLogicalExpression(aliasTable, hostMethod, resultField, UnaryLogical);
                IsAssigned = true;
                break;
            case EqualityExpression Equality:
                Result = BuildEqualityExpression(aliasTable, hostMethod, resultField, Equality);
                IsAssigned = true;
                break;
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(aliasTable, hostMethod, resultField, Comparison);
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
                Result = BuildVariableValueExpression(aliasTable, hostMethod, resultField, VariableValue);
                IsAssigned = true;
                break;
        }

        Debug.Assert(IsAssigned);

        return (T)Result;
    }

    private ArithExpr BuildBinaryArithmeticExpression(AliasTable aliasTable, Method? hostMethod, Field? resultField, BinaryArithmeticExpression binaryArithmeticExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, hostMethod, resultField, binaryArithmeticExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, hostMethod, resultField, binaryArithmeticExpression.Right);
        return OperatorBuilder.BinaryArithmetic[binaryArithmeticExpression.Operator](Context, Left, Right);
    }

    private ArithExpr BuildUnaryArithmeticExpression(AliasTable aliasTable, Method? hostMethod, Field? resultField, UnaryArithmeticExpression unaryArithmeticExpression)
    {
        ArithExpr Operand = BuildExpression<ArithExpr>(aliasTable, hostMethod, resultField, unaryArithmeticExpression.Operand);
        return OperatorBuilder.UnaryArithmetic[unaryArithmeticExpression.Operator](Context, Operand);
    }

    private BoolExpr BuildBinaryLogicalExpression(AliasTable aliasTable, Method? hostMethod, Field? resultField, BinaryLogicalExpression binaryLogicalExpression)
    {
        BoolExpr Left = BuildExpression<BoolExpr>(aliasTable, hostMethod, resultField, binaryLogicalExpression.Left);
        BoolExpr Right = BuildExpression<BoolExpr>(aliasTable, hostMethod, resultField, binaryLogicalExpression.Right);
        return OperatorBuilder.BinaryLogical[binaryLogicalExpression.Operator](Context, Left, Right);
    }

    private BoolExpr BuildUnaryLogicalExpression(AliasTable aliasTable, Method? hostMethod, Field? resultField, UnaryLogicalExpression unaryLogicalExpression)
    {
        BoolExpr Operand = BuildExpression<BoolExpr>(aliasTable, hostMethod, resultField, unaryLogicalExpression.Operand);
        return OperatorBuilder.UnaryLogical[unaryLogicalExpression.Operator](Context, Operand);
    }

    private BoolExpr BuildEqualityExpression(AliasTable aliasTable, Method? hostMethod, Field? resultField, EqualityExpression equalityExpression)
    {
        Expr Left = BuildExpression<Expr>(aliasTable, hostMethod, resultField, equalityExpression.Left);
        Expr Right = BuildExpression<Expr>(aliasTable, hostMethod, resultField, equalityExpression.Right);
        return OperatorBuilder.Equality[equalityExpression.Operator](Context, Left, Right);
    }

    private BoolExpr BuildComparisonExpression(AliasTable aliasTable, Method? hostMethod, Field? resultField, ComparisonExpression comparisonExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, hostMethod, resultField, comparisonExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, hostMethod, resultField, comparisonExpression.Right);
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

    private Expr BuildVariableValueExpression(AliasTable aliasTable, Method? hostMethod, Field? resultField, VariableValueExpression variableValueExpression)
    {
        string VariableName = variableValueExpression.VariableName.Text;
        string? VariableString = null;
        ExpressionType VariableType = ExpressionType.Other;

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
            if (Entry.Key.Text == VariableName)
            {
                Field Field = Entry.Value;
                VariableAlias FieldAlias = aliasTable.GetAlias(Field);
                VariableString = FieldAlias.ToString();
                VariableType = Field.Type;
                break;
            }

        if (hostMethod is not null)
        {
            ReadOnlyParameterTable ParameterTable = hostMethod.ParameterTable;

            foreach (KeyValuePair<ParameterName, Parameter> Entry in ParameterTable)
                if (Entry.Key.Text == VariableName)
                {
                    Parameter Parameter = Entry.Value;
                    Parameter ParameterLocal = CreateParameterLocal(hostMethod, Parameter);
                    VariableAlias ParameterAlias = aliasTable.GetAlias(ParameterLocal);
                    VariableString = ParameterAlias.ToString();
                    VariableType = Parameter.Type;
                    break;
                }
        }

        if (resultField is not null && resultField.Name.Text == VariableName)
        {
            Debug.Assert(VariableString is null);

            VariableString = resultField.Name.Text;
            VariableType = resultField.Type;
        }

        Debug.Assert(VariableString is not null);
        Debug.Assert(VariableType != ExpressionType.Other);

        return CreateVariableExpr(VariableString!, VariableType);
    }
}
