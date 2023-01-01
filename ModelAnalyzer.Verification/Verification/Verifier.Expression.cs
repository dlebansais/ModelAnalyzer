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
    private T BuildExpression<T>(AliasTable aliasTable, Method? hostMethod, Local? resultLocal, IExpression expression)
        where T : Expr
    {
        Expr Result = null!;
        bool IsAssigned = false;

        switch (expression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                Result = BuildBinaryArithmeticExpression(aliasTable, hostMethod, resultLocal, BinaryArithmetic);
                IsAssigned = true;
                break;
            case UnaryArithmeticExpression UnaryArithmetic:
                Result = BuildUnaryArithmeticExpression(aliasTable, hostMethod, resultLocal, UnaryArithmetic);
                IsAssigned = true;
                break;
            case BinaryLogicalExpression BinaryLogical:
                Result = BuildBinaryLogicalExpression(aliasTable, hostMethod, resultLocal, BinaryLogical);
                IsAssigned = true;
                break;
            case UnaryLogicalExpression UnaryLogical:
                Result = BuildUnaryLogicalExpression(aliasTable, hostMethod, resultLocal, UnaryLogical);
                IsAssigned = true;
                break;
            case EqualityExpression Equality:
                Result = BuildEqualityExpression(aliasTable, hostMethod, resultLocal, Equality);
                IsAssigned = true;
                break;
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(aliasTable, hostMethod, resultLocal, Comparison);
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
                Result = BuildVariableValueExpression(aliasTable, hostMethod, resultLocal, VariableValue);
                IsAssigned = true;
                break;
        }

        Debug.Assert(IsAssigned);

        return (T)Result;
    }

    private ArithExpr BuildBinaryArithmeticExpression(AliasTable aliasTable, Method? hostMethod, Local? resultLocal, BinaryArithmeticExpression binaryArithmeticExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, hostMethod, resultLocal, binaryArithmeticExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, hostMethod, resultLocal, binaryArithmeticExpression.Right);
        return OperatorBuilder.BinaryArithmetic[binaryArithmeticExpression.Operator](Context, Left, Right);
    }

    private ArithExpr BuildUnaryArithmeticExpression(AliasTable aliasTable, Method? hostMethod, Local? resultLocal, UnaryArithmeticExpression unaryArithmeticExpression)
    {
        ArithExpr Operand = BuildExpression<ArithExpr>(aliasTable, hostMethod, resultLocal, unaryArithmeticExpression.Operand);
        return OperatorBuilder.UnaryArithmetic[unaryArithmeticExpression.Operator](Context, Operand);
    }

    private BoolExpr BuildBinaryLogicalExpression(AliasTable aliasTable, Method? hostMethod, Local? resultLocal, BinaryLogicalExpression binaryLogicalExpression)
    {
        BoolExpr Left = BuildExpression<BoolExpr>(aliasTable, hostMethod, resultLocal, binaryLogicalExpression.Left);
        BoolExpr Right = BuildExpression<BoolExpr>(aliasTable, hostMethod, resultLocal, binaryLogicalExpression.Right);
        return OperatorBuilder.BinaryLogical[binaryLogicalExpression.Operator](Context, Left, Right);
    }

    private BoolExpr BuildUnaryLogicalExpression(AliasTable aliasTable, Method? hostMethod, Local? resultLocal, UnaryLogicalExpression unaryLogicalExpression)
    {
        BoolExpr Operand = BuildExpression<BoolExpr>(aliasTable, hostMethod, resultLocal, unaryLogicalExpression.Operand);
        return OperatorBuilder.UnaryLogical[unaryLogicalExpression.Operator](Context, Operand);
    }

    private BoolExpr BuildEqualityExpression(AliasTable aliasTable, Method? hostMethod, Local? resultLocal, EqualityExpression equalityExpression)
    {
        Expr Left = BuildExpression<Expr>(aliasTable, hostMethod, resultLocal, equalityExpression.Left);
        Expr Right = BuildExpression<Expr>(aliasTable, hostMethod, resultLocal, equalityExpression.Right);
        return OperatorBuilder.Equality[equalityExpression.Operator](Context, Left, Right);
    }

    private BoolExpr BuildComparisonExpression(AliasTable aliasTable, Method? hostMethod, Local? resultLocal, ComparisonExpression comparisonExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, hostMethod, resultLocal, comparisonExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, hostMethod, resultLocal, comparisonExpression.Right);
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

    private Expr BuildVariableValueExpression(AliasTable aliasTable, Method? hostMethod, Local? resultLocal, VariableValueExpression variableValueExpression)
    {
        FieldTable TempFieldTable = new();
        foreach (var Entry in FieldTable)
            TempFieldTable.AddItem(Entry.Value);

        ParsingContext ParsingContext = new() { FieldTable = TempFieldTable, HostMethod = hostMethod };

        string VariableName = variableValueExpression.VariableName.Text;
        string? VariableString = null;
        ExpressionType VariableType = variableValueExpression.GetExpressionType(ParsingContext, resultLocal);

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
            if (Entry.Key.Text == VariableName)
            {
                Field Field = Entry.Value;
                Variable FieldVariable = new(Field.Name, Field.Type);
                VariableAlias FieldAlias = aliasTable.GetAlias(FieldVariable);
                VariableString = FieldAlias.ToString();
                break;
            }

        if (hostMethod is not null)
        {
            ReadOnlyParameterTable ParameterTable = hostMethod.ParameterTable;

            foreach (KeyValuePair<ParameterName, Parameter> Entry in ParameterTable)
                if (Entry.Key.Text == VariableName)
                {
                    Parameter Parameter = Entry.Value;
                    ParameterName ParameterBlockName = CreateParameterBlockName(hostMethod, Parameter);
                    Variable ParameterVariable = new(ParameterBlockName, Parameter.Type);
                    VariableAlias ParameterAlias = aliasTable.GetAlias(ParameterVariable);
                    VariableString = ParameterAlias.ToString();
                    break;
                }

            ReadOnlyLocalTable LocalTable = hostMethod.LocalTable;

            foreach (KeyValuePair<LocalName, Local> Entry in LocalTable)
                if (Entry.Key.Text == VariableName)
                {
                    Local Local = Entry.Value;
                    LocalName LocalBlockName = CreateLocalBlockName(hostMethod, Local);
                    Variable LocalVariable = new(LocalBlockName, Local.Type);
                    VariableAlias LocalAlias = aliasTable.GetAlias(LocalVariable);
                    VariableString = LocalAlias.ToString();
                    break;
                }
        }

        if (VariableString is null && resultLocal is not null && resultLocal.Name.Text == VariableName)
            VariableString = resultLocal.Name.Text;

        Debug.Assert(VariableString is not null);
        Debug.Assert(VariableType != ExpressionType.Other);

        return CreateVariableExpr(VariableString!, VariableType);
    }
}
