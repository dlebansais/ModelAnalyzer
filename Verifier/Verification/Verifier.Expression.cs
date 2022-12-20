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
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(aliasTable, Comparison);
                IsAssigned = true;
                break;
            case LiteralIntValueExpression LiteralIntValue:
                Result = BuildLiteralIntValueExpression(LiteralIntValue);
                IsAssigned = true;
                break;
            case LiteralBoolValueExpression LiteralBoolValue:
                Result = BuildLiteralBoolValueExpression(LiteralBoolValue);
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

    private BoolExpr BuildComparisonExpression(AliasTable aliasTable, ComparisonExpression comparisonExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, comparisonExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, comparisonExpression.Right);
        return OperatorBuilder.Comparison[comparisonExpression.Operator](Context, Left, Right);
    }

    private ArithExpr BuildLiteralIntValueExpression(LiteralIntValueExpression literalIntValueExpression)
    {
        return Context.MkInt(literalIntValueExpression.Value);
    }

    private BoolExpr BuildLiteralBoolValueExpression(LiteralBoolValueExpression literalBoolValueExpression)
    {
        return Context.MkBool(literalBoolValueExpression.Value);
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
        }

        Debug.Assert(IsHandled);

        return Result;
    }
}
