namespace ModelAnalyzer.Verification.Test;

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public class VerifierTest
{
    [Test]
    [Category("Verification")]
    public void Verifier_BasicTest()
    {
        string ClassName = "Test";
        Verifier TestObject = new()
        {
            ClassName = ClassName,
            FieldTable = ReadOnlyFieldTable.Empty,
            MethodTable = ReadOnlyMethodTable.Empty,
            InvariantList = new(),
            MaxDepth = 0,
        };

        Assert.That(TestObject.ClassName, Is.EqualTo(ClassName));

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EmptyDepth1()
    {
        string ClassName = "Test";
        Verifier TestObject = new()
        {
            ClassName = ClassName,
            FieldTable = ReadOnlyFieldTable.Empty,
            MethodTable = ReadOnlyMethodTable.Empty,
            InvariantList = new(),
            MaxDepth = 1,
        };

        Assert.That(TestObject.ClassName, Is.EqualTo(ClassName));

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneBooleanFieldSuccess()
    {
        Verifier TestObject = CreateOneFieldVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: false);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneBooleanFieldError()
    {
        Verifier TestObject = CreateOneFieldVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: true);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType == VerificationErrorType.InvariantError);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerFieldSuccess()
    {
        Verifier TestObject = CreateOneFieldVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerFieldError()
    {
        Verifier TestObject = CreateOneFieldVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType == VerificationErrorType.InvariantError);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointFieldSuccess()
    {
        Verifier TestObject = CreateOneFieldVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointFieldError()
    {
        Verifier TestObject = CreateOneFieldVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 1.0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType == VerificationErrorType.InvariantError);
    }

    private Verifier CreateOneFieldVerifier<TValue, TExpression>(TValue invariantTestValue)
        where TExpression : Expression, ILiteralExpression<TValue>, ILiteralExpression, new()
    {
        TExpression Zero = new() { Value = invariantTestValue };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            FieldName = new FieldName { Name = FieldName },
            VariableType = Zero.ExpressionType,
            Initializer = null,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField.FieldName, TestField);

        VariableValueExpression Variable = new() { Variable = TestField };
        EqualityExpression VariableEqualZero = new EqualityExpression() { Left = Variable, Right = Zero, Operator = EqualityOperator.Equal };

        Invariant TestInvariant = new()
        {
            BooleanExpression = VariableEqualZero,
            Text = VariableEqualZero.ToString(),
        };

        List<Invariant> InvariantList = new() { TestInvariant };

        Verifier TestObject = new()
        {
            ClassName = ClassName,
            FieldTable = TestFieldTable.ToReadOnly(),
            MethodTable = ReadOnlyMethodTable.Empty,
            InvariantList = InvariantList,
            MaxDepth = 0,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneBooleanFieldWithInitializerSuccess()
    {
        Verifier TestObject = CreateOneFieldWithInitializerVerifier<bool, LiteralBooleanValueExpression>(initialValue: false, invariantTestValue: false);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneBooleanFieldWithInitializerError()
    {
        Verifier TestObject = CreateOneFieldWithInitializerVerifier<bool, LiteralBooleanValueExpression>(initialValue: true, invariantTestValue: false);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType == VerificationErrorType.InvariantError);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerFieldWithInitializerSuccess()
    {
        Verifier TestObject = CreateOneFieldWithInitializerVerifier<int, LiteralIntegerValueExpression>(initialValue: 0, invariantTestValue: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerFieldWithInitializerError()
    {
        Verifier TestObject = CreateOneFieldWithInitializerVerifier<int, LiteralIntegerValueExpression>(initialValue: 1, invariantTestValue: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType == VerificationErrorType.InvariantError);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointFieldWithInitializerSuccess()
    {
        Verifier TestObject = CreateOneFieldWithInitializerVerifier<double, LiteralFloatingPointValueExpression>(initialValue: 0.0, invariantTestValue: 0.0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointFieldWithInitializerError()
    {
        Verifier TestObject = CreateOneFieldWithInitializerVerifier<double, LiteralFloatingPointValueExpression>(initialValue: 1.0, invariantTestValue: 0.0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType == VerificationErrorType.InvariantError);
    }

    private Verifier CreateOneFieldWithInitializerVerifier<TValue, TExpression>(TValue initialValue, TValue invariantTestValue)
        where TExpression : Expression, ILiteralExpression<TValue>, ILiteralExpression, new()
    {
        TExpression Initializer = new() { Value = initialValue };
        TExpression Zero = new() { Value = invariantTestValue };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            FieldName = new FieldName { Name = FieldName },
            VariableType = Zero.ExpressionType,
            Initializer = Initializer,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField.FieldName, TestField);

        VariableValueExpression Variable = new() { Variable = TestField };
        EqualityExpression VariableEqualZero = new EqualityExpression() { Left = Variable, Right = Zero, Operator = EqualityOperator.Equal };

        Invariant TestInvariant = new()
        {
            BooleanExpression = VariableEqualZero,
            Text = VariableEqualZero.ToString(),
        };

        List<Invariant> InvariantList = new() { TestInvariant };

        Verifier TestObject = new()
        {
            ClassName = ClassName,
            FieldTable = TestFieldTable.ToReadOnly(),
            MethodTable = ReadOnlyMethodTable.Empty,
            InvariantList = InvariantList,
            MaxDepth = 0,
        };

        return TestObject;
    }
}
