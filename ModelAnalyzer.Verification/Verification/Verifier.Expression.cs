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
    private T BuildExpression<T>(VerificationContext verificationContext, IExpression expression)
        where T : Expr
    {
        Expr Result = null!;
        bool IsAssigned = false;

        switch (expression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                Result = BuildBinaryArithmeticExpression(verificationContext, BinaryArithmetic);
                IsAssigned = true;
                break;
            case UnaryArithmeticExpression UnaryArithmetic:
                Result = BuildUnaryArithmeticExpression(verificationContext, UnaryArithmetic);
                IsAssigned = true;
                break;
            case BinaryLogicalExpression BinaryLogical:
                Result = BuildBinaryLogicalExpression(verificationContext, BinaryLogical);
                IsAssigned = true;
                break;
            case UnaryLogicalExpression UnaryLogical:
                Result = BuildUnaryLogicalExpression(verificationContext, UnaryLogical);
                IsAssigned = true;
                break;
            case EqualityExpression Equality:
                Result = BuildEqualityExpression(verificationContext, Equality);
                IsAssigned = true;
                break;
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(verificationContext, Comparison);
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
                Result = BuildVariableValueExpression(verificationContext, VariableValue);
                IsAssigned = true;
                break;
        }

        Debug.Assert(IsAssigned);

        return (T)Result;
    }

    private ArithExpr BuildBinaryArithmeticExpression(VerificationContext verificationContext, BinaryArithmeticExpression binaryArithmeticExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(verificationContext, binaryArithmeticExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(verificationContext, binaryArithmeticExpression.Right);
        return OperatorBuilder.BinaryArithmetic[binaryArithmeticExpression.Operator](Context, Left, Right);
    }

    private ArithExpr BuildUnaryArithmeticExpression(VerificationContext verificationContext, UnaryArithmeticExpression unaryArithmeticExpression)
    {
        ArithExpr Operand = BuildExpression<ArithExpr>(verificationContext, unaryArithmeticExpression.Operand);
        return OperatorBuilder.UnaryArithmetic[unaryArithmeticExpression.Operator](Context, Operand);
    }

    private BoolExpr BuildBinaryLogicalExpression(VerificationContext verificationContext, BinaryLogicalExpression binaryLogicalExpression)
    {
        BoolExpr Left = BuildExpression<BoolExpr>(verificationContext, binaryLogicalExpression.Left);
        BoolExpr Right = BuildExpression<BoolExpr>(verificationContext, binaryLogicalExpression.Right);
        return OperatorBuilder.BinaryLogical[binaryLogicalExpression.Operator](Context, Left, Right);
    }

    private BoolExpr BuildUnaryLogicalExpression(VerificationContext verificationContext, UnaryLogicalExpression unaryLogicalExpression)
    {
        BoolExpr Operand = BuildExpression<BoolExpr>(verificationContext, unaryLogicalExpression.Operand);
        return OperatorBuilder.UnaryLogical[unaryLogicalExpression.Operator](Context, Operand);
    }

    private BoolExpr BuildEqualityExpression(VerificationContext verificationContext, EqualityExpression equalityExpression)
    {
        Expr Left = BuildExpression<Expr>(verificationContext, equalityExpression.Left);
        Expr Right = BuildExpression<Expr>(verificationContext, equalityExpression.Right);
        return OperatorBuilder.Equality[equalityExpression.Operator](Context, Left, Right);
    }

    private BoolExpr BuildComparisonExpression(VerificationContext verificationContext, ComparisonExpression comparisonExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(verificationContext, comparisonExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(verificationContext, comparisonExpression.Right);
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

    private Expr BuildVariableValueExpression(VerificationContext verificationContext, VariableValueExpression variableValueExpression)
    {
        AliasTable AliasTable = verificationContext.AliasTable;
        string VariableName = variableValueExpression.VariableName.Text;
        string? VariableString = null;
        ExpressionType VariableType = variableValueExpression.GetExpressionType(verificationContext);

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
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

            if (VariableString is null && verificationContext.ResultLocal is Local ResultLocal && ResultLocal.Name.Text == VariableName)
            {
                LocalName ResultLocalBlockName = CreateLocalBlockName(HostMethod, ResultLocal);
                Variable ResultLocalVariable = new(ResultLocalBlockName, ResultLocal.Type);
                VariableAlias ResultLocalAlias = AliasTable.GetAlias(ResultLocalVariable);
                VariableString = ResultLocalAlias.ToString();
            }
        }

        Debug.Assert(VariableString is not null);
        Debug.Assert(VariableType != ExpressionType.Other);

        return CreateVariableExpr(VariableString!, VariableType);
    }
}
