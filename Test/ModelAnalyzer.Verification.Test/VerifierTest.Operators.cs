namespace ModelAnalyzer.Verification.Test;

using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerAddSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(0, 1, BinaryArithmeticOperator.Add, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerAddError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(0, 1, BinaryArithmeticOperator.Add, 1, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleAddSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(0.0, 1.0, BinaryArithmeticOperator.Add, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleAddError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(0.0, 1.0, BinaryArithmeticOperator.Add, 1.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerAdd2Success()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Add, 3, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerAdd2Error()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Add, 3, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleAdd2Success()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Add, 3.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleAdd2Error()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Add, 3.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerSubtractSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(0, 1, BinaryArithmeticOperator.Subtract, -1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerSubtractError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(0, 1, BinaryArithmeticOperator.Subtract, -1, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleSubtractSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(0.0, 1.0, BinaryArithmeticOperator.Subtract, -1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleSubtractError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(0.0, 1.0, BinaryArithmeticOperator.Subtract, -1.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerSubtract2Success()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Subtract, -1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerSubtract2Error()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Subtract, -1, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleSubtract2Success()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Subtract, -1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleSubtract2Error()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Subtract, -1.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerMultiplySuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Multiply, 2, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerMultiplyError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Multiply, 2, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleMultiplySuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Multiply, 2.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleMultiplyError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Multiply, 2.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerDivideSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(2, 2, BinaryArithmeticOperator.Divide, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerDivideError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(2, 2, BinaryArithmeticOperator.Divide, 1, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleDivideSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(2.0, 2.0, BinaryArithmeticOperator.Divide, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleDivideError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(2.0, 2.0, BinaryArithmeticOperator.Divide, 1.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private Verifier CreateBinaryOperatorVerifier<TValue, TExpression>(TValue initializerValue, TValue operandValue, BinaryArithmeticOperator binaryOperator, TValue operandResult, int maxDepth)
        where TExpression : Expression, ILiteralExpression<TValue>, ILiteralExpression, new()
    {
        TExpression Zero = new() { Value = initializerValue };
        TExpression Operand = new() { Value = operandValue };
        TExpression OperandResult = new() { Value = operandResult };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            Name = new FieldName { Text = FieldName },
            Type = Zero.GetExpressionType(ReadOnlyFieldTable.Empty, null, resultField: null),
            Initializer = Zero,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariableName = TestField.Name };
        BinaryArithmeticExpression OperationExpression = new() { Left = Variable, Operator = binaryOperator, Right = Operand };
        EqualityExpression VariableEqualZero = new() { Left = Variable, Operator = EqualityOperator.Equal, Right = Zero };
        EqualityExpression VariableEqualOperand = new() { Left = Variable, Operator = EqualityOperator.Equal, Right = OperandResult };
        BinaryLogicalExpression OrExpression = new() { Left = VariableEqualZero, Operator = BinaryLogicalOperator.Or, Right = VariableEqualOperand };

        Invariant TestInvariant = new()
        {
            BooleanExpression = OrExpression,
            Location = default!,
            Text = OrExpression.ToString(),
        };

        List<Invariant> InvariantList = new() { TestInvariant };

        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, Expression = OperationExpression };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            AccessModifier = AccessModifier.Public,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            StatementList = new() { Assignment },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        Verifier TestObject = new()
        {
            ClassName = ClassName,
            FieldTable = TestFieldTable.ToReadOnly(),
            MethodTable = MethodTable.ToReadOnly(),
            InvariantList = InvariantList,
            MaxDepth = maxDepth,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerMinusSuccess()
    {
        Verifier TestObject = CreateUnaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, UnaryArithmeticOperator.Minus, -1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerMinus2Success()
    {
        Verifier TestObject = CreateUnaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, UnaryArithmeticOperator.Minus, -1, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleMinusSuccess()
    {
        Verifier TestObject = CreateUnaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, UnaryArithmeticOperator.Minus, -1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_DoubleMinus2Success()
    {
        Verifier TestObject = CreateUnaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, UnaryArithmeticOperator.Minus, -1.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private Verifier CreateUnaryOperatorVerifier<TValue, TExpression>(TValue initializerValue, UnaryArithmeticOperator unaryOperator, TValue operandResult, int maxDepth)
        where TExpression : Expression, ILiteralExpression<TValue>, ILiteralExpression, new()
    {
        TExpression Zero = new() { Value = initializerValue };
        TExpression OperandResult = new() { Value = operandResult };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            Name = new FieldName { Text = FieldName },
            Type = Zero.GetExpressionType(ReadOnlyFieldTable.Empty, null, resultField: null),
            Initializer = Zero,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariableName = TestField.Name };
        UnaryArithmeticExpression OperationExpression = new() { Operand = Variable, Operator = unaryOperator };
        EqualityExpression VariableEqualZero = new() { Left = Variable, Operator = EqualityOperator.Equal, Right = Zero };
        EqualityExpression VariableEqualOperand = new() { Left = Variable, Operator = EqualityOperator.Equal, Right = OperandResult };
        BinaryLogicalExpression OrExpression = new() { Left = VariableEqualZero, Operator = BinaryLogicalOperator.Or, Right = VariableEqualOperand };

        Invariant TestInvariant = new()
        {
            BooleanExpression = OrExpression,
            Location = default!,
            Text = OrExpression.ToString(),
        };

        List<Invariant> InvariantList = new() { TestInvariant };

        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, Expression = OperationExpression };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            AccessModifier = AccessModifier.Public,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            StatementList = new() { Assignment },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        Verifier TestObject = new()
        {
            ClassName = ClassName,
            FieldTable = TestFieldTable.ToReadOnly(),
            MethodTable = MethodTable.ToReadOnly(),
            InvariantList = InvariantList,
            MaxDepth = maxDepth,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerGTSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThan, 0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerGTEqualSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThan, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerGTError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThan, 0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerGESuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThanOrEqual, 0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerGEEqualError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThanOrEqual, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerGEError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThanOrEqual, 0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerLTSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(0, ComparisonOperator.LessThan, 1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerLTEqualSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.LessThan, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerLTError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(0, ComparisonOperator.LessThan, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerLESuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(0, ComparisonOperator.LessThanOrEqual, 1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerLEEqualError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.LessThanOrEqual, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerLEError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(0, ComparisonOperator.LessThanOrEqual, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointGTSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThan, 0.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointGTEqualSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThan, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointGTError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThan, 0.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointGESuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThanOrEqual, 0.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointGEEqualError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThanOrEqual, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointGEError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThanOrEqual, 0.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointLTSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(0, ComparisonOperator.LessThan, 1.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointLTEqualSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.LessThan, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointLTError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(0, ComparisonOperator.LessThan, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointLESuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(0, ComparisonOperator.LessThanOrEqual, 1.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointLEEqualError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.LessThanOrEqual, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointLEError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(0, ComparisonOperator.LessThanOrEqual, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private Verifier CreateComparisonOperatorVerifier<TValue, TExpression>(TValue leftValue, ComparisonOperator comparisonOperator, TValue rightValue, int maxDepth)
        where TExpression : Expression, ILiteralExpression<TValue>, ILiteralExpression, new()
    {
        LiteralBooleanValueExpression True = new() { Value = true };
        LiteralBooleanValueExpression False = new() { Value = false };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            Name = new FieldName { Text = FieldName },
            Type = ExpressionType.Boolean,
            Initializer = null,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariableName = TestField.Name };
        EqualityExpression VariableEqualFalse = new() { Left = Variable, Operator = EqualityOperator.Equal, Right = False };

        Invariant TestInvariant = new()
        {
            BooleanExpression = VariableEqualFalse,
            Location = default!,
            Text = VariableEqualFalse.ToString(),
        };

        List<Invariant> InvariantList = new() { TestInvariant };

        TExpression Left = new() { Value = leftValue };
        TExpression Right = new() { Value = rightValue };
        ComparisonExpression Comparison = new() { Left = Left, Operator = comparisonOperator, Right = Right };
        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, Expression = Comparison };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            AccessModifier = AccessModifier.Public,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            StatementList = new() { Assignment },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        Verifier TestObject = new()
        {
            ClassName = ClassName,
            FieldTable = TestFieldTable.ToReadOnly(),
            MethodTable = MethodTable.ToReadOnly(),
            InvariantList = InvariantList,
            MaxDepth = maxDepth,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanFalseOrTrueSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(false, BinaryLogicalOperator.Or, true, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanFalseOrTrueError()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(false, BinaryLogicalOperator.Or, true, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanTrueOrFalseError()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(true, BinaryLogicalOperator.Or, false, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanFalseOrFalseSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(false, BinaryLogicalOperator.Or, false, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanTrueAndTrueSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(true, BinaryLogicalOperator.And, true, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanTrueAndTrueError()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(true, BinaryLogicalOperator.And, true, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanFalseAndTrueSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(false, BinaryLogicalOperator.And, true, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanTrueAndFalseSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(true, BinaryLogicalOperator.And, false, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanFalseAndFalseSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(false, BinaryLogicalOperator.And, false, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private Verifier CreateLogicalOperatorVerifier(bool leftValue, BinaryLogicalOperator binaryOperator, bool rightValue, int maxDepth)
    {
        LiteralBooleanValueExpression True = new() { Value = true };
        LiteralBooleanValueExpression False = new() { Value = false };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            Name = new FieldName { Text = FieldName },
            Type = ExpressionType.Boolean,
            Initializer = null,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariableName = TestField.Name };
        EqualityExpression VariableEqualFalse = new() { Left = Variable, Operator = EqualityOperator.Equal, Right = False };

        Invariant TestInvariant = new()
        {
            BooleanExpression = VariableEqualFalse,
            Location = default!,
            Text = VariableEqualFalse.ToString(),
        };

        List<Invariant> InvariantList = new() { TestInvariant };

        LiteralBooleanValueExpression Left = new() { Value = leftValue };
        LiteralBooleanValueExpression Right = new() { Value = rightValue };
        BinaryLogicalExpression LogicalExpression = new() { Left = Left, Operator = binaryOperator, Right = Right };
        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, Expression = LogicalExpression };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            AccessModifier = AccessModifier.Public,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            StatementList = new() { Assignment },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        Verifier TestObject = new()
        {
            ClassName = ClassName,
            FieldTable = TestFieldTable.ToReadOnly(),
            MethodTable = MethodTable.ToReadOnly(),
            InvariantList = InvariantList,
            MaxDepth = maxDepth,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanNotSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(UnaryLogicalOperator.Not, false, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanNotError()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(UnaryLogicalOperator.Not, false, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_BooleanNotTrueSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(UnaryLogicalOperator.Not, true, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private Verifier CreateLogicalOperatorVerifier(UnaryLogicalOperator unaryOperator, bool operandValue, int maxDepth)
    {
        LiteralBooleanValueExpression True = new() { Value = true };
        LiteralBooleanValueExpression False = new() { Value = false };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            Name = new FieldName { Text = FieldName },
            Type = ExpressionType.Boolean,
            Initializer = null,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariableName = TestField.Name };
        EqualityExpression VariableEqualFalse = new() { Left = Variable, Operator = EqualityOperator.Equal, Right = False };

        Invariant TestInvariant = new()
        {
            BooleanExpression = VariableEqualFalse,
            Location = default!,
            Text = VariableEqualFalse.ToString(),
        };

        List<Invariant> InvariantList = new() { TestInvariant };

        LiteralBooleanValueExpression Operand = new() { Value = operandValue };
        UnaryLogicalExpression LogicalExpression = new() { Operator = unaryOperator, Operand = Operand };
        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, Expression = LogicalExpression };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            AccessModifier = AccessModifier.Public,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            StatementList = new() { Assignment },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        Verifier TestObject = new()
        {
            ClassName = ClassName,
            FieldTable = TestFieldTable.ToReadOnly(),
            MethodTable = MethodTable.ToReadOnly(),
            InvariantList = InvariantList,
            MaxDepth = maxDepth,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerEqualIntegerSuccess()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<int, LiteralIntegerValueExpression>(1, EqualityOperator.Equal, 1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerEqualIntegerError()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<int, LiteralIntegerValueExpression>(1, EqualityOperator.Equal, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointEqualFloatingPointSuccess()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, EqualityOperator.Equal, 1.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointEqualFloatingPointError()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, EqualityOperator.Equal, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerNotEqualIntegerSuccess()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<int, LiteralIntegerValueExpression>(1, EqualityOperator.NotEqual, 0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_IntegerNotEqualIntegerError()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<int, LiteralIntegerValueExpression>(1, EqualityOperator.NotEqual, 0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointNotEqualFloatingPointSuccess()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, EqualityOperator.NotEqual, 0.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Operators_FloatingPointNotEqualFloatingPointError()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, EqualityOperator.NotEqual, 0.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private Verifier CreateEqualityOperatorVerifier<TValue, TExpression>(TValue leftValue, EqualityOperator equalityOperator, TValue rightValue, int maxDepth)
        where TExpression : Expression, ILiteralExpression<TValue>, ILiteralExpression, new()
    {
        LiteralBooleanValueExpression True = new() { Value = true };
        LiteralBooleanValueExpression False = new() { Value = false };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            Name = new FieldName { Text = FieldName },
            Type = ExpressionType.Boolean,
            Initializer = null,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariableName = TestField.Name };
        EqualityExpression VariableEqualFalse = new() { Left = Variable, Operator = EqualityOperator.Equal, Right = False };

        Invariant TestInvariant = new()
        {
            BooleanExpression = VariableEqualFalse,
            Location = default!,
            Text = VariableEqualFalse.ToString(),
        };

        List<Invariant> InvariantList = new() { TestInvariant };

        TExpression Left = new() { Value = leftValue };
        TExpression Right = new() { Value = rightValue };
        EqualityExpression Equality = new() { Left = Left, Operator = equalityOperator, Right = Right };
        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, Expression = Equality };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            AccessModifier = AccessModifier.Public,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            StatementList = new() { Assignment },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        Verifier TestObject = new()
        {
            ClassName = ClassName,
            FieldTable = TestFieldTable.ToReadOnly(),
            MethodTable = MethodTable.ToReadOnly(),
            InvariantList = InvariantList,
            MaxDepth = maxDepth,
        };

        return TestObject;
    }
}
