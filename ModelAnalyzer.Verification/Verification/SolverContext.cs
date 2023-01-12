namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Z3;

/// <summary>
/// Represents context for Z3 solver.
/// </summary>
internal partial class SolverContext : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SolverContext"/> class.
    /// </summary>
    public SolverContext()
    {
        // Need model generation turned on.
        Context = new Context(new Dictionary<string, string>() { { "model", "true" } });
        Zero = Context.MkInt(0);
        False = Context.MkBool(false);
        True = Context.MkBool(true);
        Null = Context.MkInt(0);
    }

    /// <summary>
    /// Gets the zero constant.
    /// </summary>
    public IntExpr Zero { get; }

    /// <summary>
    /// Gets the false constant.
    /// </summary>
    public BoolExpr False { get; }

    /// <summary>
    /// Gets the true constant.
    /// </summary>
    public BoolExpr True { get; }

    /// <summary>
    /// Gets the null constant.
    /// </summary>
    public IntExpr Null { get; }

    /// <summary>
    /// Creates a new solver.
    /// </summary>
    public Solver CreateSolver()
    {
        return Context.MkSolver();
    }

    /// <summary>
    /// Creates a boolean constant.
    /// </summary>
    /// <param name="name">The constant name.</param>
    public BoolExpr CreateBooleanConstant(string name)
    {
        return Context.MkBoolConst(name);
    }

    /// <summary>
    /// Creates an integer constant.
    /// </summary>
    /// <param name="name">The constant name.</param>
    public IntExpr CreateIntegerConstant(string name)
    {
        return Context.MkIntConst(name);
    }

    /// <summary>
    /// Creates a floating point constant.
    /// </summary>
    /// <param name="name">The constant name.</param>
    public ArithExpr CreateFloatingPointConstant(string name)
    {
        return Context.MkRealConst(name);
    }

    /// <summary>
    /// Creates a reference constant.
    /// </summary>
    /// <param name="name">The constant name.</param>
    public Expr CreateReferenceConstant(string name)
    {
        return Context.MkIntConst(name);
    }

    /// <summary>
    /// Creates a boolean value.
    /// </summary>
    /// <param name="value">The value.</param>
    public BoolExpr CreateBooleanValue(bool value)
    {
        return value == false ? False : True;
    }

    /// <summary>
    /// Creates an integer value.
    /// </summary>
    /// <param name="value">The value.</param>
    public IntExpr CreateIntegerValue(int value)
    {
        return value == 0 ? Zero : Context.MkInt(value);
    }

    /// <summary>
    /// Creates a floating point value.
    /// </summary>
    /// <param name="value">The value.</param>
    public ArithExpr CreateFloatingPointValue(double value)
    {
        return value == 0 ? Zero : (ArithExpr)Context.MkNumeral(value.ToString(CultureInfo.InvariantCulture), Context.MkRealSort());
    }

    /// <summary>
    /// Creates a reference value.
    /// </summary>
    /// <param name="value">The value.</param>
    public Expr CreateReferenceValue(int value)
    {
        Debug.Assert(value > 0);
        return Context.MkInt(value);
    }

    /// <summary>
    /// Creates the equal expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public BoolExpr CreateEqualExpr(Expr left, Expr right)
    {
        return Context.MkEq(left, right);
    }

    /// <summary>
    /// Creates the not equal expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public BoolExpr CreateNotEqualExpr(Expr left, Expr right)
    {
        return Context.MkNot(Context.MkEq(left, right));
    }

    /// <summary>
    /// Creates the opposite of the provided expression.
    /// </summary>
    /// <param name="expression">The expression.</param>
    public BoolExpr CreateOppositeExpr(BoolExpr expression)
    {
        return Context.MkNot(expression);
    }

    /// <summary>
    /// Creates the + expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public ArithExpr CreateAddExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkAdd(left, right);
    }

    /// <summary>
    /// Creates the - expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public ArithExpr CreateSubtractExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkSub(left, right);
    }

    /// <summary>
    /// Creates the * expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public ArithExpr CreateMultiplyExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkMul(left, right);
    }

    /// <summary>
    /// Creates the / expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public ArithExpr CreateDivideExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkDiv(left, right);
    }

    /// <summary>
    /// Creates the remainder expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IntExpr CreateRemainderExpr(IntExpr left, IntExpr right)
    {
        return Context.MkMod(left, right);
    }

    /// <summary>
    /// Creates the - expression.
    /// </summary>
    /// <param name="operand">The operand.</param>
    public ArithExpr CreateNegateExpr(ArithExpr operand)
    {
        return Context.MkUnaryMinus(operand);
    }

    /// <summary>
    /// Creates the or expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public BoolExpr CreateOrExpr(BoolExpr left, BoolExpr right)
    {
        return Context.MkOr(left, right);
    }

    /// <summary>
    /// Creates the and expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public BoolExpr CreateAndExpr(BoolExpr left, BoolExpr right)
    {
        return Context.MkAnd(left, right);
    }

    /// <summary>
    /// Creates the not expression.
    /// </summary>
    /// <param name="operand">The operand.</param>
    public BoolExpr CreateNotExpr(BoolExpr operand)
    {
        return Context.MkNot(operand);
    }

    /// <summary>
    /// Creates the &gt; expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public BoolExpr CreateGreaterThanExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkGt(left, right);
    }

    /// <summary>
    /// Creates the &gt;= expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public BoolExpr CreateGreaterThanEqualToExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkGe(left, right);
    }

    /// <summary>
    /// Creates the &lt; expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public BoolExpr CreateLesserThanExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkLt(left, right);
    }

    /// <summary>
    /// Creates the &lt;= expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public BoolExpr CreateLesserThanEqualToExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkLe(left, right);
    }

    /// <summary>
    /// Creates a true branch expression.
    /// </summary>
    /// <param name="branch">The branch.</param>
    /// <param name="expression">The expression.</param>
    public BoolExpr CreateTrueBranchExpr(BoolExpr? branch, BoolExpr expression)
    {
        if (branch is not null)
            return Context.MkAnd(branch, expression);
        else
            return expression;
    }

    /// <summary>
    /// Creates a false branch expression.
    /// </summary>
    /// <param name="branch">The branch.</param>
    /// <param name="expression">The expression.</param>
    public BoolExpr CreateFalseBranchExpr(BoolExpr? branch, BoolExpr expression)
    {
        if (branch is not null)
            return Context.MkAnd(branch, Context.MkNot(expression));
        else
            return Context.MkNot(expression);
    }

    /// <summary>
    /// Adds an expression to a slover, conditional to a branch of code.
    /// </summary>
    /// <param name="solver">The solver.</param>
    /// <param name="branch">The branch.</param>
    /// <param name="boolExpr">The expression.</param>
    public void AddToSolver(Solver solver, BoolExpr? branch, BoolExpr boolExpr)
    {
        if (branch is not null)
            solver.Assert(Context.MkImplies(branch, boolExpr));
        else
            solver.Assert(boolExpr);
    }

    private Context Context;
}
