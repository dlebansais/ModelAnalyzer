namespace FieldWithInitializer.Test;

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
    public void Verifier_OneBooleanSuccess()
    {
        Verifier TestObject = CreateOneVerifier<bool, LiteralBooleanValueExpression>(initialValue: false, invariantTestValue: false);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneBooleanError()
    {
        Verifier TestObject = CreateOneVerifier<bool, LiteralBooleanValueExpression>(initialValue: true, invariantTestValue: false);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerSuccess()
    {
        Verifier TestObject = CreateOneVerifier<int, LiteralIntegerValueExpression>(initialValue: 0, invariantTestValue: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerError()
    {
        Verifier TestObject = CreateOneVerifier<int, LiteralIntegerValueExpression>(initialValue: 1, invariantTestValue: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointSuccess()
    {
        Verifier TestObject = CreateOneVerifier<double, LiteralFloatingPointValueExpression>(initialValue: 0.0, invariantTestValue: 0.0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointError()
    {
        Verifier TestObject = CreateOneVerifier<double, LiteralFloatingPointValueExpression>(initialValue: 1.0, invariantTestValue: 0.0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private Verifier CreateOneVerifier<TValue, TExpression>(TValue initialValue, TValue invariantTestValue)
        where TExpression : Expression, ILiteralExpression<TValue>, ILiteralExpression, new()
    {
        TExpression Initializer = new() { Value = initialValue };
        TExpression Zero = new() { Value = invariantTestValue };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            Name = new FieldName { Text = FieldName },
            Type = Zero.GetExpressionType(memberCollectionContext: null!),
            Initializer = Initializer,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariableName = TestField.Name };
        EqualityExpression VariableEqualZero = new EqualityExpression() { Left = Variable, Right = Zero, Operator = EqualityOperator.Equal };

        Invariant TestInvariant = new()
        {
            BooleanExpression = VariableEqualZero,
            Location = default!,
            Text = VariableEqualZero.ToString(),
        };

        List<Invariant> InvariantList = new() { TestInvariant };

        ClassModel ClassModel = new ClassModel()
        {
            Name = ClassName,
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
            MaxDepth = 0,
            MaxDuration = TimeSpan.MaxValue,
            ClassModelTable = new Dictionary<string, ClassModel>() { { ClassName, ClassModel } },
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = ReadOnlyMethodTable.Empty,
            InvariantList = InvariantList,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_TwoBooleanFieldsWithInitializerSuccess()
    {
        Verifier TestObject = CreateTwoFieldsWithInitializerVerifier<bool, LiteralBooleanValueExpression>(initialValue: false, invariantTestValue1: false, invariantTestValue2: false);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_TwoBooleanFieldsWithInitializerError()
    {
        Verifier TestObject = CreateTwoFieldsWithInitializerVerifier<bool, LiteralBooleanValueExpression>(initialValue: false, invariantTestValue1: true, invariantTestValue2: false);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_TwoIntegerFieldsWithInitializerSuccess()
    {
        Verifier TestObject = CreateTwoFieldsWithInitializerVerifier<int, LiteralIntegerValueExpression>(initialValue: 0, invariantTestValue1: 0, invariantTestValue2: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_TwoIntegerFieldsWithInitializerError()
    {
        Verifier TestObject = CreateTwoFieldsWithInitializerVerifier<int, LiteralIntegerValueExpression>(initialValue: 0, invariantTestValue1: 1, invariantTestValue2: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_TwoFloatingPointFieldsWithInitializerSuccess()
    {
        Verifier TestObject = CreateTwoFieldsWithInitializerVerifier<double, LiteralFloatingPointValueExpression>(initialValue: 0.0, invariantTestValue1: 0.0, invariantTestValue2: 0.0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_TwoFloatingPointFieldsWithInitializerError()
    {
        Verifier TestObject = CreateTwoFieldsWithInitializerVerifier<double, LiteralFloatingPointValueExpression>(initialValue: 0.0, invariantTestValue1: 1.0, invariantTestValue2: 0.0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private Verifier CreateTwoFieldsWithInitializerVerifier<TValue, TExpression>(TValue initialValue, TValue invariantTestValue1, TValue invariantTestValue2)
        where TExpression : Expression, ILiteralExpression<TValue>, ILiteralExpression, new()
    {
        TExpression Initializer = new() { Value = initialValue };
        TExpression Zero1 = new() { Value = invariantTestValue1 };
        TExpression Zero2 = new() { Value = invariantTestValue2 };

        string ClassName = "Test";
        string FieldName1 = "X";
        string FieldName2 = "Y";

        Field TestField1 = new()
        {
            Name = new FieldName { Text = FieldName1 },
            Type = Zero1.GetExpressionType(memberCollectionContext: null!),
            Initializer = Initializer,
        };

        Field TestField2 = new()
        {
            Name = new FieldName { Text = FieldName2 },
            Type = Zero2.GetExpressionType(memberCollectionContext: null!),
            Initializer = Initializer,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField1);
        TestFieldTable.AddItem(TestField2);

        VariableValueExpression Variable1 = new() { VariableName = TestField1.Name };
        EqualityExpression VariableEqualZero1 = new EqualityExpression() { Left = Variable1, Right = Zero1, Operator = EqualityOperator.Equal };

        VariableValueExpression Variable2 = new() { VariableName = TestField2.Name };
        EqualityExpression VariableEqualZero2 = new EqualityExpression() { Left = Variable2, Right = Zero2, Operator = EqualityOperator.Equal };

        BinaryLogicalExpression AndExpression = new() { Left = VariableEqualZero1, Operator = BinaryLogicalOperator.And, Right = VariableEqualZero2 };
        Invariant TestInvariant = new()
        {
            BooleanExpression = AndExpression,
            Location = default!,
            Text = AndExpression.ToString(),
        };

        List<Invariant> InvariantList = new() { TestInvariant };

        ClassModel ClassModel = new ClassModel()
        {
            Name = ClassName,
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
            MaxDepth = 0,
            MaxDuration = TimeSpan.MaxValue,
            ClassModelTable = new Dictionary<string, ClassModel>() { { ClassName, ClassModel } },
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = ReadOnlyMethodTable.Empty,
            InvariantList = InvariantList,
        };

        return TestObject;
    }
}
