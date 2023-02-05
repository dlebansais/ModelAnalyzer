﻿namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using AnalysisLogger;
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

        Zero = Context.MkInt(0).Encapsulate();
        False = Context.MkBool(false).Encapsulate();
        True = Context.MkBool(true).Encapsulate();
        Null = Context.MkInt(0).EncapsulateAsRef(ReferenceIndex.Null);
        ZeroSet = Zero.ToSingleSet();
        FalseSet = False.ToSingleSet();
        NullSet = Null.ToSingleSet();
    }

    /// <summary>
    /// Gets or sets the logger.
    /// </summary>
    public IAnalysisLogger Logger { get; set; } = new NullLogger();

    /// <summary>
    /// Gets the zero constant.
    /// </summary>
    public IIntExprCapsule Zero { get; }

    /// <summary>
    /// Gets the false constant.
    /// </summary>
    public IBoolExprCapsule False { get; }

    /// <summary>
    /// Gets the true constant.
    /// </summary>
    public IBoolExprCapsule True { get; }

    /// <summary>
    /// Gets the null constant.
    /// </summary>
    public IRefExprCapsule Null { get; }

    /// <summary>
    /// Gets the zero constant set.
    /// </summary>
    public IExprSet<IIntExprCapsule> ZeroSet { get; }

    /// <summary>
    /// Gets the false constant set.
    /// </summary>
    public IExprSet<IBoolExprCapsule> FalseSet { get; }

    /// <summary>
    /// Gets the null constant set.
    /// </summary>
    public IExprSet<IRefExprCapsule> NullSet { get; }

    /// <summary>
    /// Creates a new solver.
    /// </summary>
    public Solver CreateSolver()
    {
        Log("*** Creating a new solver");

        return Context.MkSolver();
    }

    /// <summary>
    /// Creates a boolean constant.
    /// </summary>
    /// <param name="name">The constant name.</param>
    public IBoolExprCapsule CreateBooleanConstant(string name)
    {
        return Context.MkBoolConst(name).Encapsulate();
    }

    /// <summary>
    /// Creates an integer constant.
    /// </summary>
    /// <param name="name">The constant name.</param>
    public IIntExprCapsule CreateIntegerConstant(string name)
    {
        return Context.MkIntConst(name).Encapsulate();
    }

    /// <summary>
    /// Creates a floating point constant.
    /// </summary>
    /// <param name="name">The constant name.</param>
    public IArithExprCapsule CreateFloatingPointConstant(string name)
    {
        return Context.MkRealConst(name).Encapsulate();
    }

    /// <summary>
    /// Creates an object reference constant.
    /// </summary>
    /// <param name="className">The class name.</param>
    /// <param name="name">The constant name.</param>
    public IObjectRefExprCapsule CreateObjectReferenceConstant(ClassName className, string name)
    {
        return Context.MkIntConst(name).EncapsulateAsObjectRef(className, ReferenceIndex.Null);
    }

    /// <summary>
    /// Creates an array reference constant.
    /// </summary>
    /// <param name="elementType">The element type.</param>
    /// <param name="name">The constant name.</param>
    public IArrayRefExprCapsule CreateArrayReferenceConstant(ExpressionType elementType, string name)
    {
        Debug.Assert(!elementType.IsArray);

        return Context.MkIntConst(name).EncapsulateAsArrayRef(elementType, ReferenceIndex.Null);
    }

    /// <summary>
    /// Creates a boolean value.
    /// </summary>
    /// <param name="value">The value.</param>
    public IBoolExprCapsule CreateBooleanValue(bool value)
    {
        return value == false ? False : True;
    }

    /// <summary>
    /// Creates an integer value.
    /// </summary>
    /// <param name="value">The value.</param>
    public IIntExprCapsule CreateIntegerValue(int value)
    {
        return value == 0 ? Zero : Context.MkInt(value).Encapsulate();
    }

    /// <summary>
    /// Creates a floating point value.
    /// </summary>
    /// <param name="value">The value.</param>
    public IArithExprCapsule CreateFloatingPointValue(double value)
    {
        return value == 0 ? Zero : ((ArithExpr)Context.MkNumeral(value.ToString(CultureInfo.InvariantCulture), Context.MkRealSort())).Encapsulate();
    }

    /// <summary>
    /// Creates an object reference value.
    /// </summary>
    /// <param name="className">The class name.</param>
    /// <param name="index">The reference index.</param>
    public IObjectRefExprCapsule CreateObjectReferenceValue(ClassName className, ReferenceIndex index)
    {
        Debug.Assert(index != ReferenceIndex.Null);
        return Context.MkInt(index.Internal).EncapsulateAsObjectRef(className, index);
    }

    /// <summary>
    /// Creates an array reference value.
    /// </summary>
    /// <param name="elementType">The element type.</param>
    /// <param name="index">The reference index.</param>
    public IArrayRefExprCapsule CreateArrayReferenceValue(ExpressionType elementType, ReferenceIndex index)
    {
        Debug.Assert(!elementType.IsArray);
        Debug.Assert(index != ReferenceIndex.Null);

        return Context.MkInt(index.Internal).EncapsulateAsArrayRef(elementType, index);
    }

    /// <summary>
    /// Creates an array value.
    /// </summary>
    /// <param name="elementType">The element type.</param>
    /// <param name="arraySize">The array size.</param>
    public IArrayExprCapsule CreateArrayValue(ExpressionType elementType, ArraySize arraySize)
    {
        Debug.Assert(!elementType.IsArray);
        Debug.Assert(arraySize.IsValid);

        Dictionary<ExpressionType, Sort> DomainTable = new()
        {
            { ExpressionType.Boolean, Context.BoolSort },
            { ExpressionType.Integer, Context.IntSort },
            { ExpressionType.FloatingPoint, Context.RealSort },
        };

        Dictionary<ExpressionType, IExprCapsule> DefaultvalueTable = new()
        {
            { ExpressionType.Boolean, False },
            { ExpressionType.Integer, Zero },
            { ExpressionType.FloatingPoint, Zero },
        };

        if (elementType.IsSimple)
        {
            Debug.Assert(DomainTable.ContainsKey(elementType));
            Debug.Assert(DefaultvalueTable.ContainsKey(elementType));

            return Context.MkConstArray(DomainTable[elementType], DefaultvalueTable[elementType].Item).Encapsulate();
        }
        else
        {
            return Context.MkConstArray(Context.IntSort, Zero.Item).Encapsulate();
        }
    }

    /// <summary>
    /// Creates an equal expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateEqualExpr(Expr left, Expr right)
    {
        return Context.MkEq(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates a not equal expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateNotEqualExpr(Expr left, Expr right)
    {
        return Context.MkNot(Context.MkEq(left, right)).Encapsulate();
    }

    /// <summary>
    /// Creates the + expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IArithExprCapsule CreateAddExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkAdd(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates the - expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IArithExprCapsule CreateSubtractExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkSub(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates the * expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IArithExprCapsule CreateMultiplyExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkMul(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates the / expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IArithExprCapsule CreateDivideExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkDiv(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates the remainder expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IIntExprCapsule CreateRemainderExpr(IntExpr left, IntExpr right)
    {
        return Context.MkMod(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates the - expression.
    /// </summary>
    /// <param name="operand">The operand.</param>
    public IArithExprCapsule CreateNegateExpr(ArithExpr operand)
    {
        return Context.MkUnaryMinus(operand).Encapsulate();
    }

    /// <summary>
    /// Creates the or expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateOrExpr(BoolExpr left, BoolExpr right)
    {
        return Context.MkOr(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates the and expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateAndExpr(BoolExpr left, BoolExpr right)
    {
        return Context.MkAnd(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates the not expression.
    /// </summary>
    /// <param name="operand">The operand.</param>
    public IBoolExprCapsule CreateNotExpr(BoolExpr operand)
    {
        return Context.MkNot(operand).Encapsulate();
    }

    /// <summary>
    /// Creates the &gt; expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateGreaterThanExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkGt(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates the &gt;= expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateGreaterThanEqualToExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkGe(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates the &lt; expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateLesserThanExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkLt(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates the &lt;= expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateLesserThanEqualToExpr(ArithExpr left, ArithExpr right)
    {
        return Context.MkLe(left, right).Encapsulate();
    }

    /// <summary>
    /// Creates a set of equal expressions.
    /// </summary>
    /// <param name="left">The left operands.</param>
    /// <param name="right">The right operands.</param>
    public IExprSet<IBoolExprCapsule> CreateEqualExprSet(IExprBase<IExprCapsule, IExprCapsule> left, IExprBase<IExprCapsule, IExprCapsule> right)
    {
        Debug.Assert(left.OtherExpressions.Count == right.OtherExpressions.Count);
        int Count = left.OtherExpressions.Count;

        List<IBoolExprCapsule> EqualityExprList = new();
        IBoolExprCapsule EqualityExpr = Context.MkEq(left.MainExpression.Item, right.MainExpression.Item).Encapsulate();
        EqualityExprList.Add(EqualityExpr);

        for (int i = 0; i < Count; i++)
        {
            EqualityExpr = Context.MkEq(left.OtherExpressions[i].Item, right.OtherExpressions[i].Item).Encapsulate();
            EqualityExprList.Add(EqualityExpr);
        }

        ExprSet<IBoolExprCapsule> Result = new(EqualityExprList);

        return Result;
    }

    /// <summary>
    /// Creates a set of not equal expressions.
    /// </summary>
    /// <param name="left">The left operands.</param>
    /// <param name="right">The right operands.</param>
    public IExprSet<IBoolExprCapsule> CreateNotEqualExprSet(IExprBase<IExprCapsule, IExprCapsule> left, IExprBase<IExprCapsule, IExprCapsule> right)
    {
        Debug.Assert(left.OtherExpressions.Count == right.OtherExpressions.Count);
        int Count = left.OtherExpressions.Count;

        List<IBoolExprCapsule> EqualityExprList = new();
        IBoolExprCapsule EqualityExpr = Context.MkNot(Context.MkEq(left.MainExpression.Item, right.MainExpression.Item)).Encapsulate();
        EqualityExprList.Add(EqualityExpr);

        for (int i = 0; i < Count; i++)
        {
            EqualityExpr = Context.MkNot(Context.MkEq(left.OtherExpressions[i].Item, right.OtherExpressions[i].Item)).Encapsulate();
            EqualityExprList.Add(EqualityExpr);
        }

        ExprSet<IBoolExprCapsule> Result = new(EqualityExprList);

        return Result;
    }

    /// <summary>
    /// Creates a set of opposites of the provided expressions.
    /// </summary>
    /// <param name="expressionSet">The set of expressions.</param>
    public IExprSet<IBoolExprCapsule> CreateOppositeExprSet(IExprSet<IBoolExprCapsule> expressionSet)
    {
        int Count = expressionSet.OtherExpressions.Count;

        List<IBoolExprCapsule> NotExprList = new();
        IBoolExprCapsule NotExpr = Context.MkNot(expressionSet.MainExpression.Item).Encapsulate();
        NotExprList.Add(NotExpr);

        for (int i = 0; i < Count; i++)
        {
            NotExpr = Context.MkNot(expressionSet.OtherExpressions[i].Item).Encapsulate();
            NotExprList.Add(NotExpr);
        }

        ExprSet<IBoolExprCapsule> Result = new(NotExprList);

        return Result;
    }

    /// <summary>
    /// Creates a true branch expression.
    /// </summary>
    /// <param name="branch">The branch.</param>
    /// <param name="expression">The expression.</param>
    public IBoolExprCapsule CreateTrueBranchExpr(IBoolExprCapsule? branch, IBoolExprCapsule expression)
    {
        if (branch is not null)
            return Context.MkAnd(branch.Item, expression.Item).Encapsulate();
        else
            return expression;
    }

    /// <summary>
    /// Creates a false branch expression.
    /// </summary>
    /// <param name="branch">The branch.</param>
    /// <param name="expression">The expression.</param>
    public IBoolExprCapsule CreateFalseBranchExpr(IBoolExprCapsule? branch, IBoolExprCapsule expression)
    {
        if (branch is not null)
            return Context.MkAnd(branch.Item, Context.MkNot(expression.Item)).Encapsulate();
        else
            return Context.MkNot(expression.Item).Encapsulate();
    }

    /// <summary>
    /// Adds expressions expression to a solver, conditional to a branch of code.
    /// </summary>
    /// <param name="solver">The solver.</param>
    /// <param name="branch">The branch.</param>
    /// <param name="boolExpr">The expressions.</param>
    public void AddToSolver(Solver solver, IBoolExprCapsule? branch, IExprSet<IBoolExprCapsule> boolExpr)
    {
        foreach (IBoolExprCapsule Expression in boolExpr.AllExpressions)
        {
            BoolExpr Expr;

            if (branch is not null)
                Expr = Context.MkImplies(branch.Item, Expression.Item);
            else
                Expr = Expression.Item;

            Log($"Adding {Expr}");
            solver.Assert(Expr);
        }
    }

    private void Log(string message)
    {
        Debug.WriteLine(message);

        Logger.Log(message);
    }

    private Context Context;
}
