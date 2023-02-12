namespace ModelAnalyzer;

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
    public IExprSingle<IIntExprCapsule> ZeroSet { get; }

    /// <summary>
    /// Gets the false constant set.
    /// </summary>
    public IExprSingle<IBoolExprCapsule> FalseSet { get; }

    /// <summary>
    /// Gets the null constant set.
    /// </summary>
    public IExprSingle<IRefExprCapsule> NullSet { get; }

    /// <summary>
    /// Creates a new solver.
    /// </summary>
    public Solver CreateSolver()
    {
        Log("*** Creating a new solver");

        return Context.MkSolver();
    }

    /// <summary>
    /// Creates a boolean variable.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public IBoolExprCapsule CreateBooleanVariable(string name)
    {
        return Context.MkBoolConst(name).Encapsulate();
    }

    /// <summary>
    /// Creates an integer variable.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public IIntExprCapsule CreateIntegerVariable(string name)
    {
        return Context.MkIntConst(name).Encapsulate();
    }

    /// <summary>
    /// Creates a floating point variable.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public IArithExprCapsule CreateFloatingPointVariable(string name)
    {
        return Context.MkRealConst(name).Encapsulate();
    }

    /// <summary>
    /// Creates an object reference variable.
    /// </summary>
    /// <param name="className">The class name.</param>
    /// <param name="name">The variable name.</param>
    public IObjectRefExprCapsule CreateObjectReferenceVariable(ClassName className, string name)
    {
        return Context.MkIntConst(name).EncapsulateAsObjectRef(className, ReferenceIndex.Null);
    }

    /// <summary>
    /// Creates an array reference variable.
    /// </summary>
    /// <param name="elementType">The element type.</param>
    /// <param name="name">The variable name.</param>
    public IArrayRefExprCapsule CreateArrayReferenceVariable(ExpressionType elementType, string name)
    {
        Debug.Assert(!elementType.IsArray);

        return Context.MkIntConst(name).EncapsulateAsArrayRef(elementType, ReferenceIndex.Null);
    }

    /// <summary>
    /// Creates an array container variable.
    /// </summary>
    /// <param name="elementType">The element type.</param>
    /// <param name="name">The variable name.</param>
    public IArrayExprCapsule CreateArrayContainerVariable(ExpressionType elementType, string name)
    {
        Dictionary<ExpressionType, Sort> DomainTable = new()
        {
            { ExpressionType.Boolean, Context.BoolSort },
            { ExpressionType.Integer, Context.IntSort },
            { ExpressionType.FloatingPoint, Context.RealSort },
        };

        Debug.Assert(!elementType.IsArray);
        Debug.Assert(elementType.IsSimple);
        Debug.Assert(DomainTable.ContainsKey(elementType));

        Sort Domain = DomainTable[elementType];

        return Context.MkArrayConst(name, Context.IntSort, Domain).Encapsulate(elementType);
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
        Debug.Assert(elementType.IsSimple);
        Debug.Assert(arraySize.IsValid);

        Dictionary<ExpressionType, Func<Expr>> DefaultvalueTable = new()
        {
            { ExpressionType.Boolean, () => Context.MkBool(false) },
            { ExpressionType.Integer, () => Context.MkInt(0) },
            { ExpressionType.FloatingPoint, () => Context.MkReal(0) },
        };

        Debug.Assert(DefaultvalueTable.ContainsKey(elementType));

        return Context.MkConstArray(Context.IntSort, DefaultvalueTable[elementType]()).Encapsulate(elementType);
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
        Debug.Assert(Result.MainExpression == Result.AllExpressions[0]);
        Debug.Assert(Result.OtherExpressions.Count + 1 == Result.AllExpressions.Count);

        return Result;
    }

    /// <summary>
    /// Creates a set of greater than or equal to expressions.
    /// </summary>
    /// <param name="left">The left operands.</param>
    /// <param name="right">The right operands.</param>
    public IExprSet<IBoolExprCapsule> CreateGreaterThanOrEqualToExprSet(IExprSingle<IArithExprCapsule> left, IExprSingle<IArithExprCapsule> right)
    {
        IBoolExprCapsule EqualityExpr = Context.MkGe(left.MainExpression.Item, right.MainExpression.Item).Encapsulate();

        ExprSingle<IBoolExprCapsule> Result = new(EqualityExpr);

        return Result;
    }

    /// <summary>
    /// Creates a set of not equal expressions.
    /// </summary>
    /// <param name="left">The left operands.</param>
    /// <param name="right">The right operands.</param>
    public IExprSingle<IBoolExprCapsule> CreateNotEqualExprSet(IExprSingle<IExprCapsule> left, IExprSingle<IExprCapsule> right)
    {
        IBoolExprCapsule EqualityExpr = Context.MkNot(Context.MkEq(left.MainExpression.Item, right.MainExpression.Item)).Encapsulate();
        ExprSingle<IBoolExprCapsule> Result = new(EqualityExpr);

        return Result;
    }

    /// <summary>
    /// Creates a set of opposites of the provided expressions.
    /// </summary>
    /// <param name="expressionSet">The set of expressions.</param>
    public IExprSingle<IBoolExprCapsule> CreateOppositeExprSet(IExprSingle<IBoolExprCapsule> expressionSet)
    {
        IBoolExprCapsule NotExpr = Context.MkNot(expressionSet.MainExpression.Item).Encapsulate();
        ExprSingle<IBoolExprCapsule> Result = new(NotExpr);

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
    /// Creates a get element expression.
    /// </summary>
    /// <param name="arrayExpr">The array.</param>
    /// <param name="indexExpr">The index.</param>
    public IExprCapsule CreateGetElementExpr(IArrayExprCapsule arrayExpr, IIntExprCapsule indexExpr)
    {
        ExpressionType ElementType = arrayExpr.ElementType;
        Dictionary<ExpressionType, Func<IArrayExprCapsule, IExprCapsule>> CreateTable = new()
        {
            { ExpressionType.Boolean, (IArrayExprCapsule arrayExpr) => ((BoolExpr)Context.MkSelect(arrayExpr.Item, indexExpr.Item)).Encapsulate() },
            { ExpressionType.Integer, (IArrayExprCapsule arrayExpr) => ((IntExpr)Context.MkSelect(arrayExpr.Item, indexExpr.Item)).Encapsulate() },
            { ExpressionType.FloatingPoint, (IArrayExprCapsule arrayExpr) => ((ArithExpr)Context.MkSelect(arrayExpr.Item, indexExpr.Item)).Encapsulate() },
        };

        Debug.Assert(ElementType.IsSimple);
        Debug.Assert(CreateTable.ContainsKey(ElementType));

        IExprCapsule Result = CreateTable[ElementType](arrayExpr);

        return Result;
    }

    /// <summary>
    /// Creates a set element expression.
    /// </summary>
    /// <param name="arrayExpr">The array.</param>
    /// <param name="indexExpr">The index.</param>
    /// <param name="valueExpr">The value.</param>
    public IArrayExprCapsule CreateSetElementExpr(IArrayExprCapsule arrayExpr, IIntExprCapsule indexExpr, IExprCapsule valueExpr)
    {
        ExpressionType ElementType = arrayExpr.ElementType;
        Dictionary<ExpressionType, Func<IArrayExprCapsule, IExprCapsule, IArrayExprCapsule>> CreateTable = new()
        {
            { ExpressionType.Boolean, (IArrayExprCapsule arrayExpr, IExprCapsule valueExpr) => Context.MkStore(arrayExpr.Item, indexExpr.Item, valueExpr.Item).Encapsulate(ExpressionType.Boolean) },
            { ExpressionType.Integer, (IArrayExprCapsule arrayExpr, IExprCapsule valueExpr) => Context.MkStore(arrayExpr.Item, indexExpr.Item, valueExpr.Item).Encapsulate(ExpressionType.Integer) },
            { ExpressionType.FloatingPoint, (IArrayExprCapsule arrayExpr, IExprCapsule valueExpr) => Context.MkStore(arrayExpr.Item, indexExpr.Item, valueExpr.Item).Encapsulate(ExpressionType.FloatingPoint) },
        };

        Debug.Assert(ElementType.IsSimple);
        Debug.Assert(CreateTable.ContainsKey(ElementType));

        IArrayExprCapsule Result = CreateTable[ElementType](arrayExpr, valueExpr);

        return Result;
    }

    /// <summary>
    /// Adds boolean expressions to a solver, conditional to a branch of code.
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
