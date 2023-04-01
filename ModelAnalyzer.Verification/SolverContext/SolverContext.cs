namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using AnalysisLogger;
using CodeProverBinding;

/// <summary>
/// Represents context for Z3 solver.
/// </summary>
internal partial class SolverContext : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SolverContext"/> class.
    /// </summary>
    /// <param name="binder">The binder.</param>
    public SolverContext(Binder binder)
    {
        Binder = binder;

        Zero = Binder.Zero.Encapsulate();
        False = Binder.False.Encapsulate();
        True = Binder.True.Encapsulate();
        Null = Binder.Null.EncapsulateAsRef(Reference.Null);
        ZeroSet = Zero.ToSingleSet();
        FalseSet = False.ToSingleSet();
        NullSet = Null.ToSingleSet();
    }

    /// <summary>
    /// Gets the binder.
    /// </summary>
    public Binder Binder { get; }

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
    /// Creates a boolean variable.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public IBoolExprCapsule CreateBooleanVariable(string name)
    {
        return Binder.CreateBooleanSymbolExpression(name).Encapsulate();
    }

    /// <summary>
    /// Creates an integer variable.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public IIntExprCapsule CreateIntegerVariable(string name)
    {
        return Binder.CreateIntegerSymbolExpression(name).Encapsulate();
    }

    /// <summary>
    /// Creates a floating point variable.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public IArithExprCapsule CreateFloatingPointVariable(string name)
    {
        return Binder.CreateFloatingPointSymbolExpression(name).Encapsulate();
    }

    /// <summary>
    /// Creates an object reference variable.
    /// </summary>
    /// <param name="className">The class name.</param>
    /// <param name="name">The variable name.</param>
    public IObjectRefExprCapsule CreateObjectReferenceVariable(ClassName className, string name)
    {
        return Binder.CreateObjectReferenceSymbolExpression(name).EncapsulateAsObjectRef(className, Reference.Null);
    }

    /// <summary>
    /// Creates an array reference variable.
    /// </summary>
    /// <param name="elementType">The element type.</param>
    /// <param name="name">The variable name.</param>
    public IArrayRefExprCapsule CreateArrayReferenceVariable(ExpressionType elementType, string name)
    {
        Debug.Assert(!elementType.IsArray);

        return Binder.CreateArrayReferenceSymbolExpression(name).EncapsulateAsArrayRef(elementType, Reference.Null);
    }

    /// <summary>
    /// Creates an array container variable.
    /// </summary>
    /// <param name="elementType">The element type.</param>
    /// <param name="name">The variable name.</param>
    public IArrayExprCapsule CreateArrayContainerVariable(ExpressionType elementType, string name)
    {
        Dictionary<ExpressionType, CodeProverBinding.ISort> DomainTable = new()
        {
            { ExpressionType.Boolean, CodeProverBinding.Sort.Boolean },
            { ExpressionType.Integer, CodeProverBinding.Sort.Integer },
            { ExpressionType.FloatingPoint, CodeProverBinding.Sort.FloatingPoint },
        };

        Debug.Assert(!elementType.IsArray);
        Debug.Assert(elementType.IsSimple);
        Debug.Assert(DomainTable.ContainsKey(elementType));

        CodeProverBinding.ISort Domain = DomainTable[elementType];

        return Binder.CreateXxxArraySymbolExpression(name, Domain).Encapsulate(elementType);
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
        return value == 0 ? Zero : Binder.GetIntegerConstant(value).Encapsulate();
    }

    /// <summary>
    /// Creates a floating point value.
    /// </summary>
    /// <param name="value">The value.</param>
    public IArithExprCapsule CreateFloatingPointValue(double value)
    {
        return value == 0 ? Zero : Binder.GetFloatingPointConstant(value).Encapsulate();
    }

    /// <summary>
    /// Creates an object reference value.
    /// </summary>
    /// <param name="className">The class name.</param>
    /// <param name="index">The reference index.</param>
    public IObjectRefExprCapsule CreateObjectReferenceValue(ClassName className, Reference index)
    {
        Debug.Assert(index != Reference.Null);
        return Binder.GetReferenceConstant(index).EncapsulateAsObjectRef(className, index);
    }

    /// <summary>
    /// Creates an array reference value.
    /// </summary>
    /// <param name="elementType">The element type.</param>
    /// <param name="index">The reference index.</param>
    public IArrayRefExprCapsule CreateArrayReferenceValue(ExpressionType elementType, Reference index)
    {
        Debug.Assert(!elementType.IsArray);
        Debug.Assert(index != Reference.Null);

        return Binder.GetReferenceConstant(index).EncapsulateAsArrayRef(elementType, index);
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

        Dictionary<ExpressionType, CodeProverBinding.IExpression> DefaultvalueTable = new()
        {
            { ExpressionType.Boolean, Binder.False },
            { ExpressionType.Integer, Binder.Zero },
            { ExpressionType.FloatingPoint, Binder.FloatingPointZero },
        };

        Debug.Assert(DefaultvalueTable.ContainsKey(elementType));

        return Binder.GetXxxArrayConstantExpression(DefaultvalueTable[elementType]).Encapsulate(elementType);
    }

    /// <summary>
    /// Creates an equal expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateEqualExpr(IExprCapsule left, IExprCapsule right)
    {
        return Binder.CreateEqualityExpression(left.Item, CodeProverBinding.EqualityOperator.Equal, right.Item).Encapsulate();
    }

    /// <summary>
    /// Creates a not equal expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateNotEqualExpr(IExprCapsule left, IExprCapsule right)
    {
        return Binder.CreateEqualityExpression(left.Item, CodeProverBinding.EqualityOperator.NotEqual, right.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the + expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IArithExprCapsule CreateAddExpr(IArithExprCapsule left, IArithExprCapsule right)
    {
        return Binder.CreateBinaryArithmeticExpression(left.Item, CodeProverBinding.BinaryArithmeticOperator.Add, right.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the - expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IArithExprCapsule CreateSubtractExpr(IArithExprCapsule left, IArithExprCapsule right)
    {
        return Binder.CreateBinaryArithmeticExpression(left.Item, CodeProverBinding.BinaryArithmeticOperator.Subtract, right.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the * expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IArithExprCapsule CreateMultiplyExpr(IArithExprCapsule left, IArithExprCapsule right)
    {
        return Binder.CreateBinaryArithmeticExpression(left.Item, CodeProverBinding.BinaryArithmeticOperator.Multiply, right.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the / expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IArithExprCapsule CreateDivideExpr(IArithExprCapsule left, IArithExprCapsule right)
    {
        return Binder.CreateBinaryArithmeticExpression(left.Item, CodeProverBinding.BinaryArithmeticOperator.Divide, right.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the remainder expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IIntExprCapsule CreateRemainderExpr(IIntExprCapsule left, IIntExprCapsule right)
    {
        return ((IIntegerExpression)Binder.CreateBinaryArithmeticExpression(left.Item, CodeProverBinding.BinaryArithmeticOperator.Modulo, right.Item)).Encapsulate();
    }

    /// <summary>
    /// Creates the - expression.
    /// </summary>
    /// <param name="operand">The operand.</param>
    public IArithExprCapsule CreateNegateExpr(IArithExprCapsule operand)
    {
        return Binder.CreateUnaryArithmeticExpression(CodeProverBinding.UnaryArithmeticOperator.Minus, operand.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the or expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateOrExpr(IBoolExprCapsule left, IBoolExprCapsule right)
    {
        return Binder.CreateBinaryLogicalExpression(left.Item, CodeProverBinding.BinaryLogicalOperator.Or, right.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the and expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateAndExpr(IBoolExprCapsule left, IBoolExprCapsule right)
    {
        return Binder.CreateBinaryLogicalExpression(left.Item, CodeProverBinding.BinaryLogicalOperator.And, right.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the not expression.
    /// </summary>
    /// <param name="operand">The operand.</param>
    public IBoolExprCapsule CreateNotExpr(IBoolExprCapsule operand)
    {
        return Binder.CreateUnaryLogicalExpression(CodeProverBinding.UnaryLogicalOperator.Not, operand.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the &gt; expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateGreaterThanExpr(IArithExprCapsule left, IArithExprCapsule right)
    {
        return Binder.CreateComparisonExpression(left.Item, CodeProverBinding.ComparisonOperator.GreaterThan, right.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the &gt;= expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateGreaterThanEqualToExpr(IArithExprCapsule left, IArithExprCapsule right)
    {
        return Binder.CreateComparisonExpression(left.Item, CodeProverBinding.ComparisonOperator.GreaterThanEqualTo, right.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the &lt; expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateLesserThanExpr(IArithExprCapsule left, IArithExprCapsule right)
    {
        return Binder.CreateComparisonExpression(left.Item, CodeProverBinding.ComparisonOperator.LesserThan, right.Item).Encapsulate();
    }

    /// <summary>
    /// Creates the &lt;= expression.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    public IBoolExprCapsule CreateLesserThanEqualToExpr(IArithExprCapsule left, IArithExprCapsule right)
    {
        return Binder.CreateComparisonExpression(left.Item, CodeProverBinding.ComparisonOperator.LesserThanEqualTo, right.Item).Encapsulate();
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
        IBoolExprCapsule EqualityExpr = Binder.CreateEqualityExpression(left.MainExpression.Item, CodeProverBinding.EqualityOperator.Equal, right.MainExpression.Item).Encapsulate();
        EqualityExprList.Add(EqualityExpr);

        for (int i = 0; i < Count; i++)
        {
            EqualityExpr = Binder.CreateEqualityExpression(left.OtherExpressions[i].Item, CodeProverBinding.EqualityOperator.Equal, right.OtherExpressions[i].Item).Encapsulate();
            EqualityExprList.Add(EqualityExpr);
        }

        ExprSet<IBoolExprCapsule> Result = new(EqualityExprList);
        Debug.Assert(Result.MainExpression == Result.AllExpressions[0]);
        Debug.Assert(Result.OtherExpressions.Count + 1 == Result.AllExpressions.Count);

        return Result;
    }

    /// <summary>
    /// Creates a set of not equal expressions.
    /// </summary>
    /// <param name="left">The left operands.</param>
    /// <param name="right">The right operands.</param>
    public IExprSingle<IBoolExprCapsule> CreateNotEqualExprSet(IExprSingle<IExprCapsule> left, IExprSingle<IExprCapsule> right)
    {
        IBoolExprCapsule EqualityExpr = Binder.CreateEqualityExpression(left.MainExpression.Item, CodeProverBinding.EqualityOperator.NotEqual, right.MainExpression.Item).Encapsulate();
        ExprSingle<IBoolExprCapsule> Result = new(EqualityExpr);

        return Result;
    }

    /// <summary>
    /// Creates a set of opposites of the provided expressions.
    /// </summary>
    /// <param name="expressionSet">The set of expressions.</param>
    public IExprSingle<IBoolExprCapsule> CreateOppositeExprSet(IExprSingle<IBoolExprCapsule> expressionSet)
    {
        IBoolExprCapsule NotExpr = Binder.CreateUnaryLogicalExpression(CodeProverBinding.UnaryLogicalOperator.Not, expressionSet.MainExpression.Item).Encapsulate();
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
            return Binder.CreateBinaryLogicalExpression(branch.Item, CodeProverBinding.BinaryLogicalOperator.And, expression.Item).Encapsulate();
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
            return Binder.CreateBinaryLogicalExpression(branch.Item, CodeProverBinding.BinaryLogicalOperator.And, Binder.CreateUnaryLogicalExpression(CodeProverBinding.UnaryLogicalOperator.Not, expression.Item)).Encapsulate();
        else
            return Binder.CreateUnaryLogicalExpression(CodeProverBinding.UnaryLogicalOperator.Not, expression.Item).Encapsulate();
    }

    /// <summary>
    /// Adds boolean expressions to a solver, conditional to a branch of code.
    /// </summary>
    /// <param name="branch">The branch.</param>
    /// <param name="boolExpr">The expressions.</param>
    public void AddToSolver(IBoolExprCapsule? branch, IExprSet<IBoolExprCapsule> boolExpr)
    {
        foreach (IBoolExprCapsule Expression in boolExpr.AllExpressions)
        {
            IBooleanExpression Expr;

            if (branch is not null)
                Expr = Binder.CreateBinaryLogicalExpression(branch.Item, CodeProverBinding.BinaryLogicalOperator.Implies, Expression.Item);
            else
                Expr = Expression.Item;

            Log($"Adding {Expr}");
            Expr.Assert();
        }
    }

    private void Log(string message)
    {
        Debug.WriteLine(message);

        Logger.Log(message);
    }
}
