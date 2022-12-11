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
                Result = BuildBinaryExpression(aliasTable, BinaryArithmetic);
                IsAssigned = true;
                break;
            case BinaryConditionalExpression BinaryLogical:
                Result = BuildBinaryConditionalExpression(aliasTable, BinaryLogical);
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
            case ParenthesizedExpression Parenthesized:
                Result = BuildParenthesizedExpression(aliasTable, Parenthesized);
                IsAssigned = true;
                break;
            case VariableValueExpression VariableValue:
                Result = BuildVariableValueExpression(aliasTable, VariableValue);
                IsAssigned = true;
                break;
        }

        Debug.Assert(IsAssigned);

        return Result as T ?? throw new InvalidOperationException($"Expected expression of type {typeof(T).Name}");
    }

    private ArithExpr BuildBinaryExpression(AliasTable aliasTable, BinaryArithmeticExpression binaryArithmeticExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, binaryArithmeticExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, binaryArithmeticExpression.Right);
        return binaryArithmeticExpression.Operator.Asserter(ctx, Left, Right);
    }

    private BoolExpr BuildBinaryConditionalExpression(AliasTable aliasTable, BinaryConditionalExpression binaryLogicalExpression)
    {
        BoolExpr Left = BuildExpression<BoolExpr>(aliasTable, binaryLogicalExpression.Left);
        BoolExpr Right = BuildExpression<BoolExpr>(aliasTable, binaryLogicalExpression.Right);
        return binaryLogicalExpression.Operator.Asserter(ctx, Left, Right);
    }

    private BoolExpr BuildComparisonExpression(AliasTable aliasTable, ComparisonExpression comparisonExpression)
    {
        ArithExpr Left = BuildExpression<ArithExpr>(aliasTable, comparisonExpression.Left);
        ArithExpr Right = BuildExpression<ArithExpr>(aliasTable, comparisonExpression.Right);
        return comparisonExpression.Operator.Asserter(ctx, Left, Right);
    }

    private ArithExpr BuildLiteralIntValueExpression(LiteralIntValueExpression literalIntValueExpression)
    {
        return ctx.MkInt(literalIntValueExpression.Value);
    }

    private BoolExpr BuildLiteralBoolValueExpression(LiteralBoolValueExpression literalBoolValueExpression)
    {
        return ctx.MkBool(literalBoolValueExpression.Value);
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
