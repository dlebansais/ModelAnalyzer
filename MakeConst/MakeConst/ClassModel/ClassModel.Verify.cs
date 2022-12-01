namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Z3;

public partial record ClassModel
{
    public void Verify()
    {
        const int MaxDepth = 0;
        bool IsInvariantViolated = Verify(MaxDepth, 0);

        ClassModelManager.Instance.SetIsInvariantViolated(Name, IsInvariantViolated);

        Logger.Log("Pulsing event");
        PulseEvent.Set();
    }

    public bool Verify(int maxDepth, int depth)
    {
        // Need model generation turned on.
        using Context ctx = new Context(new Dictionary<string, string>() { { "model", "true" } });
        bool IsInvariantViolated = Verify(ctx, depth);

        if (IsInvariantViolated)
            return true;

        if (depth < maxDepth)
            return Verify(maxDepth, depth + 1);
        else
            return false;
    }

    public void WaitForThreadCompleted()
    {
        bool IsCompleted = PulseEvent.WaitOne(TimeSpan.FromSeconds(2));

        Logger.Log($"Wait on event done, IsCompleted={IsCompleted}");
    }

    public void AddInitialState(Context ctx, Solver solver, Dictionary<string, string> aliasTable)
    {
        IntExpr Zero = ctx.MkInt(0);

        foreach (KeyValuePair<FieldName, IField> Entry in FieldTable)
        {
            string FieldName = Entry.Key.Name;
            string FieldNameAlias = $"{FieldName}_x";
            aliasTable.Add(FieldName, FieldNameAlias);

            IntExpr FieldExpr = ctx.MkIntConst(FieldNameAlias);
            solver.Assert(ctx.MkEq(FieldExpr, Zero));
            Logger.Log($"Adding {FieldName} == 0");
        }
    }

    public static Dictionary<SyntaxKind, ComparisonOperator> SupportedComparisonOperators = new()
    {
        { SyntaxKind.EqualsEqualsToken, new ComparisonOperator("==", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkEq(left, right)) },
        { SyntaxKind.ExclamationEqualsToken, new ComparisonOperator("!=", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkNot(ctx.MkEq(left, right))) },
        { SyntaxKind.GreaterThanToken, new ComparisonOperator(">", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkGt(left, right)) },
        { SyntaxKind.GreaterThanEqualsToken, new ComparisonOperator(">=", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkGe(left, right)) },
        { SyntaxKind.LessThanToken, new ComparisonOperator("<", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkLt(left, right)) },
        { SyntaxKind.LessThanEqualsToken, new ComparisonOperator("<=", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkLe(left, right)) },
    };

    public static Dictionary<SyntaxKind, ArithmeticOperator> SupportedArithmeticOperators = new()
    {
        { SyntaxKind.PlusToken, new ArithmeticOperator("+", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkAdd(left, right)) },
        { SyntaxKind.MinusToken, new ArithmeticOperator("-", (Context ctx, ArithExpr left, ArithExpr right) => ctx.MkSub(left, right)) },
    };

    public static Dictionary<SyntaxKind, LogicalOperator> SupportedLogicalOperators = new()
    {
        { SyntaxKind.BarBarToken, new LogicalOperator("||", (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkOr(left, right)) },
        { SyntaxKind.AmpersandAmpersandToken, new LogicalOperator("&&", (Context ctx, BoolExpr left, BoolExpr right) => ctx.MkAnd(left, right)) },
    };

    public void AddInvariants(Context ctx, Solver solver, Dictionary<string, string> aliasTable)
    {
        List<BoolExpr> AssertionList = new();

        foreach (IInvariant Item in InvariantList)
            if (Item is Invariant Invariant)
            {
                string FieldName = Invariant.FieldName;
                string FieldNameAlias = aliasTable[FieldName];

                SyntaxKind OperatorKind = Invariant.OperatorKind;
                int ConstantValue = Invariant.ConstantValue;

                if (SupportedComparisonOperators.ContainsKey(OperatorKind))
                {
                    ComparisonOperator Operator = SupportedComparisonOperators[OperatorKind];
                    Logger.Log($"Adding {FieldName} {Operator.Text} {ConstantValue}");

                    IntExpr FieldExpr = ctx.MkIntConst(FieldNameAlias);
                    IntExpr ConstantExpr = ctx.MkInt(ConstantValue);
                    BoolExpr Assertion = Operator.Asserter(ctx, FieldExpr, ConstantExpr);
                    AssertionList.Add(Assertion);
                }
                else
                    throw new NotImplementedException($"Comparison operator {OperatorKind} not implemented");
            }

        solver.Assert(ctx.MkAnd(AssertionList));
    }

    public bool Verify(Context ctx, int depth)
    {
        Logger.Log("Solver starting");
        Solver solver = ctx.MkSolver();

        Dictionary<string, string> AliasTable = new();
        AddInitialState(ctx, solver, AliasTable);
        CallMethods(ctx, solver, AliasTable, depth);
        AddInvariants(ctx, solver, AliasTable);

        bool IsInvariantViolated = solver.Check() != Status.SATISFIABLE;
        Logger.Log($"Solver result: {IsInvariantViolated}");

        return IsInvariantViolated;
    }

    public void CallMethods(Context ctx, Solver solver, Dictionary<string, string> aliasTable, int depth)
    {
        if (depth == 0)
            return;

        foreach (KeyValuePair<MethodName, IMethod> Entry in MethodTable)
            if (Entry.Value is Method Method)
            {
                AddMethodCallState(ctx, solver, aliasTable, Method);

                CallMethods(ctx, solver, aliasTable, depth - 1);
            }
    }

    public void AddMethodCallState(Context ctx, Solver solver, Dictionary<string, string> aliasTable, Method method)
    {
        List<IntExpr> ExpressionList = new();

        foreach (KeyValuePair<ParameterName, IParameter> Entry in method.ParameterTable)
            if (Entry.Value is Parameter Parameter)
            {
                string ParameterName = Parameter.Name;
                string ParameterNameAlias = $"{ParameterName}_x";
                aliasTable.Add(ParameterName, ParameterNameAlias);

                IntExpr ParameterExpr = ctx.MkIntConst(ParameterNameAlias);
                ExpressionList.Add(ParameterExpr);
            }

        BoolExpr True = ctx.MkBool(true);
        
        AddStatementListExecution(ctx, solver, aliasTable, True, method.StatementList);
    }

    public void AddStatementListExecution(Context ctx, Solver solver, Dictionary<string, string> aliasTable, BoolExpr branch, List<IStatement> statementList)
    {
        foreach (IStatement Statement in statementList)
            switch (Statement)
            {
                case AssignmentStatement Assignment:
                    AddAssignmentExecution(ctx, solver, aliasTable, branch, Assignment);
                    break;
                case ConditionalStatement Conditional:
                    AddConditionalExecution(ctx, solver, aliasTable, branch, Conditional);
                    break;
                case ReturnStatement Return:
                    AddReturnExecution(ctx, solver, aliasTable, branch, Return);
                    break;
                case UnsupportedStatement:
                default:
                    throw new InvalidOperationException("Unexpected unsupported statement");
            }
    }

    public void AddAssignmentExecution(Context ctx, Solver solver, Dictionary<string, string> aliasTable, BoolExpr branch, AssignmentStatement assignmentStatement)
    {
        string DestinationName = assignmentStatement.Destination.Name;
        string DestinationNameAlias = $"{aliasTable[DestinationName]}x";
        aliasTable[DestinationName] = DestinationNameAlias;

        IntExpr DestinationExpr = ctx.MkIntConst(DestinationNameAlias);
        Expr SourceExpr = BuildExpression(ctx, aliasTable, assignmentStatement.Expression);

        solver.Assert(ctx.MkAnd(branch, ctx.MkEq(DestinationExpr, SourceExpr)));
    }

    public void AddConditionalExecution(Context ctx, Solver solver, Dictionary<string, string> aliasTable, BoolExpr branch, ConditionalStatement conditionalStatement)
    {
        BoolExpr ConditionExpr = BuildExpression(ctx, aliasTable, conditionalStatement.Condition) as BoolExpr ?? throw new InvalidOperationException("Expected boolean expression");
        AddStatementListExecution(ctx, solver, aliasTable, ConditionExpr, conditionalStatement.WhenTrueStatementList);
        AddStatementListExecution(ctx, solver, aliasTable, ctx.MkNot(ConditionExpr), conditionalStatement.WhenFalseStatementList);
    }

    public void AddReturnExecution(Context ctx, Solver solver, Dictionary<string, string> aliasTable, BoolExpr branch, ReturnStatement returnStatement)
    {
    }

    public Expr BuildExpression(Context ctx, Dictionary<string, string> aliasTable, IExpression expression)
    {
        switch (expression)
        {
            case BinaryArithmeticExpression BinaryArithmetic:
                return BuildBinaryExpression(ctx, aliasTable, BinaryArithmetic);
            case BinaryLogicalExpression BinaryLogical:
                return BuildBinaryLogicalExpression(ctx, aliasTable, BinaryLogical);
            case ComparisonExpression Comparison:
                return BuildComparisonExpression(ctx, aliasTable, Comparison);
            case LiteralValueExpression LiteralValue:
                return BuildLiteralValueExpression(ctx, LiteralValue);
            case VariableValueExpression VariableValue:
                return BuildVariableValueExpression(ctx, aliasTable, VariableValue);
            case UnsupportedExpression:
            default:
                throw new InvalidOperationException("Unexpected unsupported expression");
        }
    }

    public ArithExpr BuildBinaryExpression(Context ctx, Dictionary<string, string> aliasTable, BinaryArithmeticExpression binaryArithmeticExpression)
    {
        ArithExpr Left = BuildExpression(ctx, aliasTable, binaryArithmeticExpression.Left) as ArithExpr ?? throw new InvalidOperationException("Expected arithmetic expression");
        ArithExpr Right = BuildExpression(ctx, aliasTable, binaryArithmeticExpression.Right) as ArithExpr ?? throw new InvalidOperationException("Expected arithmetic expression");
        return SupportedArithmeticOperators[binaryArithmeticExpression.OperatorKind].Asserter(ctx, Left, Right);
    }

    public BoolExpr BuildBinaryLogicalExpression(Context ctx, Dictionary<string, string> aliasTable, BinaryLogicalExpression binaryLogicalExpression)
    {
        BoolExpr Left = BuildExpression(ctx, aliasTable, binaryLogicalExpression.Left) as BoolExpr ?? throw new InvalidOperationException("Expected boolean expression");
        BoolExpr Right = BuildExpression(ctx, aliasTable, binaryLogicalExpression.Right) as BoolExpr ?? throw new InvalidOperationException("Expected boolean expression");
        return SupportedLogicalOperators[binaryLogicalExpression.OperatorKind].Asserter(ctx, Left, Right);
    }

    public BoolExpr BuildComparisonExpression(Context ctx, Dictionary<string, string> aliasTable, ComparisonExpression comparisonExpression)
    {
        ArithExpr Left = BuildExpression(ctx, aliasTable, comparisonExpression.Left) as ArithExpr ?? throw new InvalidOperationException("Expected arithmetic expression");
        ArithExpr Right = BuildExpression(ctx, aliasTable, comparisonExpression.Right) as ArithExpr ?? throw new InvalidOperationException("Expected arithmetic expression");
        return SupportedComparisonOperators[comparisonExpression.OperatorKind].Asserter(ctx, Left, Right);
    }

    public ArithExpr BuildLiteralValueExpression(Context ctx, LiteralValueExpression literalValueExpression)
    {
        return ctx.MkInt(literalValueExpression.Value);
    }

    public ArithExpr BuildVariableValueExpression(Context ctx, Dictionary<string, string> aliasTable, VariableValueExpression variableValueExpression)
    {
        string VariableName = variableValueExpression.Variable.Name;
        string VariableAliasName = aliasTable[VariableName];
        return ctx.MkIntConst(VariableAliasName);
    }
}
