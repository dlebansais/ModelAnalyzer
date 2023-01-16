namespace SimpleField.Test;

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
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
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
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
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
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private Verifier CreateOneFieldVerifier<TValue, TExpression>(TValue invariantTestValue)
        where TExpression : Expression, ILiteralExpression<TValue>, ILiteralExpression, new()
    {
        TExpression Zero = new() { Value = invariantTestValue };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            Name = new FieldName { Text = FieldName },
            Type = Zero.GetExpressionType(),
            Initializer = null,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariablePath = new List<IVariable>() { TestField }, PathLocation = null! };
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
}
