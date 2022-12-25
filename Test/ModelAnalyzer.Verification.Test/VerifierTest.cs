namespace ModelAnalyzer.Verification.Test;

using System.Collections.Generic;
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
            FieldTable = new(),
            MethodTable = new(),
            InvariantList = new(),
            MaxDepth = 0,
        };

        Assert.That(TestObject.ClassName, Is.EqualTo(ClassName));

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EmptyDepth1()
    {
        string ClassName = "Test";
        Verifier TestObject = new()
        {
            ClassName = ClassName,
            FieldTable = new(),
            MethodTable = new(),
            InvariantList = new(),
            MaxDepth = 1,
        };

        Assert.That(TestObject.ClassName, Is.EqualTo(ClassName));

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFieldSuccess()
    {
        Verifier TestObject = CreateOneFieldVerifier(invariantTestValue: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFieldError()
    {
        Verifier TestObject = CreateOneFieldVerifier(invariantTestValue: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError);
        Assert.That(VerificationResult.ErrorType == VerificationErrorType.InvariantError);
    }

    private Verifier CreateOneFieldVerifier(int invariantTestValue)
    {
        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            FieldName = new FieldName { Name = FieldName },
            VariableType = ExpressionType.Integer,
            Initializer = null,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField.FieldName, TestField);
        TestFieldTable.Seal();

        VariableValueExpression Variable = new() { Variable = TestField };
        LiteralIntegerValueExpression Zero = new() { Value = invariantTestValue };
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
            FieldTable = TestFieldTable,
            MethodTable = new(),
            InvariantList = InvariantList,
            MaxDepth = 0,
        };

        return TestObject;
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFieldWithInitializerSuccess()
    {
        Verifier TestObject = CreateOneFieldWithInitializerVerifier(initialValue: 0, invariantTestValue: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFieldWithInitializerError()
    {
        Verifier TestObject = CreateOneFieldWithInitializerVerifier(initialValue: 1, invariantTestValue: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError);
        Assert.That(VerificationResult.ErrorType == VerificationErrorType.InvariantError);
    }

    private Verifier CreateOneFieldWithInitializerVerifier(int initialValue, int invariantTestValue)
    {
        LiteralIntegerValueExpression Initializer = new() { Value = initialValue };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            FieldName = new FieldName { Name = FieldName },
            VariableType = ExpressionType.Integer,
            Initializer = Initializer,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField.FieldName, TestField);
        TestFieldTable.Seal();

        VariableValueExpression Variable = new() { Variable = TestField };
        LiteralIntegerValueExpression Zero = new() { Value = invariantTestValue };
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
            FieldTable = TestFieldTable,
            MethodTable = new(),
            InvariantList = InvariantList,
            MaxDepth = 0,
        };

        return TestObject;
    }
}
