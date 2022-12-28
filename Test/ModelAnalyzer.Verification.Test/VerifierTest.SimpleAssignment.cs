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
    public void Verifier_SimpleAssignment_OneBooleanFieldSuccess()
    {
        Verifier TestObject = CreateSimpleAssignmentVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: false, assignmentValue: true, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_SimpleAssignment_OneBooleanFieldError()
    {
        Verifier TestObject = CreateSimpleAssignmentVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: true, assignmentValue: true, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_SimpleAssignment_OneBooleanFieldWithWriteError()
    {
        Verifier TestObject = CreateSimpleAssignmentVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: false, assignmentValue: true, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_SimpleAssignment_OneIntegerFieldSuccess()
    {
        Verifier TestObject = CreateSimpleAssignmentVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 0, assignmentValue: 1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_SimpleAssignment_OneIntegerFieldError()
    {
        Verifier TestObject = CreateSimpleAssignmentVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 1, assignmentValue: 1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_SimpleAssignment_OneIntegerFieldWithWriteError()
    {
        Verifier TestObject = CreateSimpleAssignmentVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 0, assignmentValue: 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_SimpleAssignment_OneFloatingPointFieldSuccess()
    {
        Verifier TestObject = CreateSimpleAssignmentVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 0.0, assignmentValue: 1.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_SimpleAssignment_OneFloatingPointFieldError()
    {
        Verifier TestObject = CreateSimpleAssignmentVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 1.0, assignmentValue: 1.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_SimpleAssignment_OneFloatingPointFieldWithWriteError()
    {
        Verifier TestObject = CreateSimpleAssignmentVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 0.0, assignmentValue: 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private Verifier CreateSimpleAssignmentVerifier<TValue, TExpression>(TValue invariantTestValue, TValue assignmentValue, int maxDepth)
        where TExpression : Expression, ILiteralExpression<TValue>, ILiteralExpression, new()
    {
        TExpression Zero = new() { Value = invariantTestValue };
        TExpression AssignmentSource = new() { Value = assignmentValue };

        string ClassName = "Test";
        string FieldName = "X";
        Field TestField = new()
        {
            Name = new FieldName { Text = FieldName },
            Type = Zero.GetExpressionType(ReadOnlyFieldTable.Empty, ReadOnlyParameterTable.Empty, resultField: null),
            Initializer = null,
        };

        FieldTable TestFieldTable = new();
        TestFieldTable.AddItem(TestField);

        VariableValueExpression Variable = new() { VariableName = TestField.Name };
        EqualityExpression VariableEqualZero = new EqualityExpression() { Left = Variable, Right = Zero, Operator = EqualityOperator.Equal };

        Invariant TestInvariant = new()
        {
            BooleanExpression = VariableEqualZero,
            Text = VariableEqualZero.ToString(),
        };

        List<Invariant> InvariantList = new() { TestInvariant };

        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, Expression = AssignmentSource };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            RequireList = new(),
            ParameterTable = ReadOnlyParameterTable.Empty,
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
