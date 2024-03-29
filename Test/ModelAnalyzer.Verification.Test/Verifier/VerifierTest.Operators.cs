﻿namespace Operators.Test;

using System;
using System.Collections.Generic;
using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    [Test]
    [Category("Verification")]
    public void Verifier_IntegerAddSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(0, 1, BinaryArithmeticOperator.Add, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerAddError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(0, 1, BinaryArithmeticOperator.Add, 1, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleAddSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(0.0, 1.0, BinaryArithmeticOperator.Add, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleAddError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(0.0, 1.0, BinaryArithmeticOperator.Add, 1.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerAdd2Success()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Add, 3, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerAdd2Error()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Add, 3, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleAdd2Success()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Add, 3.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleAdd2Error()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Add, 3.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerSubtractSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(0, 1, BinaryArithmeticOperator.Subtract, -1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerSubtractError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(0, 1, BinaryArithmeticOperator.Subtract, -1, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleSubtractSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(0.0, 1.0, BinaryArithmeticOperator.Subtract, -1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleSubtractError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(0.0, 1.0, BinaryArithmeticOperator.Subtract, -1.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerSubtract2Success()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Subtract, -1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerSubtract2Error()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Subtract, -1, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleSubtract2Success()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Subtract, -1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleSubtract2Error()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Subtract, -1.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerMultiplySuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Multiply, 2, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerMultiplyError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, 2, BinaryArithmeticOperator.Multiply, 2, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleMultiplySuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Multiply, 2.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleMultiplyError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, 2.0, BinaryArithmeticOperator.Multiply, 2.0, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerDivideSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(2, 2, BinaryArithmeticOperator.Divide, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerDivideError()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<int, LiteralIntegerValueExpression>(2, 2, BinaryArithmeticOperator.Divide, 1, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleDivideSuccess()
    {
        Verifier TestObject = CreateBinaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(2.0, 2.0, BinaryArithmeticOperator.Divide, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleDivideError()
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

        ClassName ClassName = ClassName.FromSimpleString("Test");
        string FieldName = "X";
        Field TestField = new(new FieldName { Text = FieldName }, Zero.GetExpressionType())
        {
            Initializer = Zero,
            ClassName = ClassName,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariablePath = new List<IVariable>() { TestField }, PathLocation = null! };
        BinaryArithmeticExpression OperationExpression = new() { Left = Variable, Operator = binaryOperator, Right = Operand, Location = null! };
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

        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, DestinationIndex = null, Expression = OperationExpression };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            ClassName = ClassName,
            AccessModifier = AccessModifier.Public,
            IsStatic = false,
            IsPreloaded = false,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            RootBlock = new BlockScope() { LocalTable = ReadOnlyLocalTable.Empty, IndexLocal = null, ContinueCondition = null, StatementList = new() { Assignment } },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        ClassModel ClassModel = new ClassModel()
        {
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = ReadOnlyMethodTable.Empty,
            InvariantList = InvariantList,
            Unsupported = new Unsupported(),
            InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
            RequireViolations = new List<IRequireViolation>().AsReadOnly(),
            EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
            AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
        };

        Verifier TestObject = new()
        {
            MaxDepth = maxDepth,
            MaxDuration = TimeSpan.MaxValue,
            ClassModelTable = new ClassModelTable() { { ClassName, ClassModel } },
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerMinusSuccess()
    {
        Verifier TestObject = CreateUnaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, UnaryArithmeticOperator.Minus, -1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerMinus2Success()
    {
        Verifier TestObject = CreateUnaryOperatorVerifier<int, LiteralIntegerValueExpression>(1, UnaryArithmeticOperator.Minus, -1, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleMinusSuccess()
    {
        Verifier TestObject = CreateUnaryOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, UnaryArithmeticOperator.Minus, -1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_DoubleMinus2Success()
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

        ClassName ClassName = ClassName.FromSimpleString("Test");
        string FieldName = "X";
        Field TestField = new(new FieldName { Text = FieldName }, Zero.GetExpressionType())
        {
            Initializer = Zero,
            ClassName = ClassName,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariablePath = new List<IVariable>() { TestField }, PathLocation = null! };
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

        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, DestinationIndex = null, Expression = OperationExpression };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            ClassName = ClassName,
            AccessModifier = AccessModifier.Public,
            IsStatic = false,
            IsPreloaded = false,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            RootBlock = new BlockScope() { LocalTable = ReadOnlyLocalTable.Empty, IndexLocal = null, ContinueCondition = null, StatementList = new() { Assignment } },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        ClassModel ClassModel = new ClassModel()
        {
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
            Unsupported = new Unsupported(),
            InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
            RequireViolations = new List<IRequireViolation>().AsReadOnly(),
            EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
            AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
        };

        Verifier TestObject = new()
        {
            MaxDepth = maxDepth,
            MaxDuration = TimeSpan.MaxValue,
            ClassModelTable = new ClassModelTable() { { ClassName, ClassModel } },
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerGTSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThan, 0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerGTEqualSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThan, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerGTError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThan, 0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerGESuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThanOrEqual, 0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerGEEqualError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThanOrEqual, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerGEError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.GreaterThanOrEqual, 0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerLTSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(0, ComparisonOperator.LessThan, 1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerLTEqualSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.LessThan, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerLTError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(0, ComparisonOperator.LessThan, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerLESuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(0, ComparisonOperator.LessThanOrEqual, 1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerLEEqualError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(1, ComparisonOperator.LessThanOrEqual, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerLEError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<int, LiteralIntegerValueExpression>(0, ComparisonOperator.LessThanOrEqual, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointGTSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThan, 0.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointGTEqualSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThan, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointGTError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThan, 0.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointGESuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThanOrEqual, 0.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointGEEqualError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThanOrEqual, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointGEError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.GreaterThanOrEqual, 0.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointLTSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(0, ComparisonOperator.LessThan, 1.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointLTEqualSuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.LessThan, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointLTError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(0, ComparisonOperator.LessThan, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointLESuccess()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(0, ComparisonOperator.LessThanOrEqual, 1.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointLEEqualError()
    {
        Verifier TestObject = CreateComparisonOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, ComparisonOperator.LessThanOrEqual, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointLEError()
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

        ClassName ClassName = ClassName.FromSimpleString("Test");
        string FieldName = "X";
        Field TestField = new(new FieldName { Text = FieldName }, ExpressionType.Boolean)
        {
            Initializer = null,
            ClassName = ClassName,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariablePath = new List<IVariable>() { TestField }, PathLocation = null! };
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
        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, DestinationIndex = null, Expression = Comparison };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            ClassName = ClassName,
            AccessModifier = AccessModifier.Public,
            IsStatic = false,
            IsPreloaded = false,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            RootBlock = new BlockScope() { LocalTable = ReadOnlyLocalTable.Empty, IndexLocal = null, ContinueCondition = null, StatementList = new() { Assignment } },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        ClassModel ClassModel = new ClassModel()
        {
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
            Unsupported = new Unsupported(),
            InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
            RequireViolations = new List<IRequireViolation>().AsReadOnly(),
            EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
            AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
        };

        Verifier TestObject = new()
        {
            MaxDepth = maxDepth,
            MaxDuration = TimeSpan.MaxValue,
            ClassModelTable = new ClassModelTable() { { ClassName, ClassModel } },
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanFalseOrTrueSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(false, BinaryLogicalOperator.Or, true, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanFalseOrTrueError()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(false, BinaryLogicalOperator.Or, true, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanTrueOrFalseError()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(true, BinaryLogicalOperator.Or, false, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanFalseOrFalseSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(false, BinaryLogicalOperator.Or, false, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanTrueAndTrueSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(true, BinaryLogicalOperator.And, true, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanTrueAndTrueError()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(true, BinaryLogicalOperator.And, true, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanFalseAndTrueSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(false, BinaryLogicalOperator.And, true, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanTrueAndFalseSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(true, BinaryLogicalOperator.And, false, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanFalseAndFalseSuccess()
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

        ClassName ClassName = ClassName.FromSimpleString("Test");
        string FieldName = "X";
        Field TestField = new(new FieldName { Text = FieldName }, ExpressionType.Boolean)
        {
            Initializer = null,
            ClassName = ClassName,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariablePath = new List<IVariable>() { TestField }, PathLocation = null! };
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
        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, DestinationIndex = null, Expression = LogicalExpression };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            ClassName = ClassName,
            AccessModifier = AccessModifier.Public,
            IsStatic = false,
            IsPreloaded = false,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            RootBlock = new BlockScope() { LocalTable = ReadOnlyLocalTable.Empty, IndexLocal = null, ContinueCondition = null, StatementList = new() { Assignment } },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        ClassModel ClassModel = new ClassModel()
        {
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
            Unsupported = new Unsupported(),
            InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
            RequireViolations = new List<IRequireViolation>().AsReadOnly(),
            EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
            AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
        };

        Verifier TestObject = new()
        {
            MaxDepth = maxDepth,
            MaxDuration = TimeSpan.MaxValue,
            ClassModelTable = new ClassModelTable() { { ClassName, ClassModel } },
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanNotSuccess()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(UnaryLogicalOperator.Not, false, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanNotError()
    {
        Verifier TestObject = CreateLogicalOperatorVerifier(UnaryLogicalOperator.Not, false, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_BooleanNotTrueSuccess()
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

        ClassName ClassName = ClassName.FromSimpleString("Test");
        string FieldName = "X";
        Field TestField = new(new FieldName { Text = FieldName }, ExpressionType.Boolean)
        {
            Initializer = null,
            ClassName = ClassName,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariablePath = new List<IVariable>() { TestField }, PathLocation = null! };
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
        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, DestinationIndex = null, Expression = LogicalExpression };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            ClassName = ClassName,
            AccessModifier = AccessModifier.Public,
            IsStatic = false,
            IsPreloaded = false,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            RootBlock = new BlockScope() { LocalTable = ReadOnlyLocalTable.Empty, IndexLocal = null, ContinueCondition = null, StatementList = new() { Assignment } },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        ClassModel ClassModel = new ClassModel()
        {
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
            Unsupported = new Unsupported(),
            InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
            RequireViolations = new List<IRequireViolation>().AsReadOnly(),
            EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
            AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
        };

        Verifier TestObject = new()
        {
            MaxDepth = maxDepth,
            MaxDuration = TimeSpan.MaxValue,
            ClassModelTable = new ClassModelTable() { { ClassName, ClassModel } },
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerEqualIntegerSuccess()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<int, LiteralIntegerValueExpression>(1, EqualityOperator.Equal, 1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerEqualIntegerError()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<int, LiteralIntegerValueExpression>(1, EqualityOperator.Equal, 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointEqualFloatingPointSuccess()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, EqualityOperator.Equal, 1.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointEqualFloatingPointError()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, EqualityOperator.Equal, 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerNotEqualIntegerSuccess()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<int, LiteralIntegerValueExpression>(1, EqualityOperator.NotEqual, 0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_IntegerNotEqualIntegerError()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<int, LiteralIntegerValueExpression>(1, EqualityOperator.NotEqual, 0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointNotEqualFloatingPointSuccess()
    {
        Verifier TestObject = CreateEqualityOperatorVerifier<double, LiteralFloatingPointValueExpression>(1.0, EqualityOperator.NotEqual, 0.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPointNotEqualFloatingPointError()
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

        ClassName ClassName = ClassName.FromSimpleString("Test");
        string FieldName = "X";
        Field TestField = new(new FieldName { Text = FieldName }, ExpressionType.Boolean)
        {
            Initializer = null,
            ClassName = ClassName,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariablePath = new List<IVariable>() { TestField }, PathLocation = null! };
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
        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, DestinationIndex = null, Expression = Equality };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            ClassName = ClassName,
            AccessModifier = AccessModifier.Public,
            IsStatic = false,
            IsPreloaded = false,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            RootBlock = new BlockScope() { LocalTable = ReadOnlyLocalTable.Empty, IndexLocal = null, ContinueCondition = null, StatementList = new() { Assignment } },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        ClassModel ClassModel = new ClassModel()
        {
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
            Unsupported = new Unsupported(),
            InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
            RequireViolations = new List<IRequireViolation>().AsReadOnly(),
            EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
            AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
        };

        Verifier TestObject = new()
        {
            MaxDepth = maxDepth,
            MaxDuration = TimeSpan.MaxValue,
            ClassModelTable = new ClassModelTable() { { ClassName, ClassModel } },
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
        };

        return TestObject;
    }
}
