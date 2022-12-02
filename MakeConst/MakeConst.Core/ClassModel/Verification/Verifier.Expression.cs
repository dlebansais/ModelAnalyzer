namespace DemoAnalyzer;

using Microsoft.Z3;
using System;

public partial class Verifier : IDisposable
{
    private T BuildExpression<T>(AliasTable aliasTable, IExpression expression)
        where T : Expr
    {
        Expr Result;

        switch (expression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                Result = BuildBinaryExpression(aliasTable, BinaryArithmetic);
                break;
            case BinaryLogicalExpression BinaryLogical:
                Result = BuildBinaryLogicalExpression(aliasTable, BinaryLogical);
                break;
            case ComparisonExpression Comparison:
                Result = BuildComparisonExpression(aliasTable, Comparison);
                break;
            case LiteralValueExpression LiteralValue:
                Result = BuildLiteralValueExpression(LiteralValue);
                break;
            case ParenthesizedExpression Parenthesized:
                Result = BuildParenthesizedExpression(aliasTable, Parenthesized);
                break;
            case VariableValueExpression VariableValue:
                Result = BuildVariableValueExpression(aliasTable, VariableValue);
                break;
            case UnsupportedExpression:
            default:
                throw new InvalidOperationException("Unexpected unsupported expression");
        }

        return Result as T ?? throw new InvalidOperationException($"Expected expression of type {typeof(T).Name}");
    }

    private ArithExpr BuildBinaryExpression(AliasTable aliasTable, BinaryArithmeticExpression binaryArithmeticExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, binaryArithmeticExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, binaryArithmeticExpression.Right);
        return ClassModel.SupportedArithmeticOperators[binaryArithmeticExpression.OperatorKind].Asserter(ctx, Left, Right);
    }

    private BoolExpr BuildBinaryLogicalExpression(AliasTable aliasTable, BinaryLogicalExpression binaryLogicalExpression)
    {
        BoolExpr Left = BuildExpression<BoolExpr>(aliasTable, binaryLogicalExpression.Left);
        BoolExpr Right = BuildExpression<BoolExpr>(aliasTable, binaryLogicalExpression.Right);
        return ClassModel.SupportedLogicalOperators[binaryLogicalExpression.OperatorKind].Asserter(ctx, Left, Right);
    }

    private BoolExpr BuildComparisonExpression(AliasTable aliasTable, ComparisonExpression comparisonExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, comparisonExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, comparisonExpression.Right);
        return ClassModel.SupportedComparisonOperators[comparisonExpression.OperatorKind].Asserter(ctx, Left, Right);
    }

    private ArithExpr BuildLiteralValueExpression(LiteralValueExpression literalValueExpression)
    {
        return ctx.MkInt(literalValueExpression.Value);
    }

    private Expr BuildParenthesizedExpression(AliasTable aliasTable, ParenthesizedExpression parenthesizedExpression)
    {
        return BuildExpression<Expr>(aliasTable, parenthesizedExpression.Inside);
    }

    private ArithExpr BuildVariableValueExpression(AliasTable aliasTable, VariableValueExpression variableValueExpression)
    {
        string VariableName = variableValueExpression.Variable.Name;
        string VariableAliasName = aliasTable.GetAlias(VariableName);

        return ctx.MkIntConst(VariableAliasName);
    }
}
