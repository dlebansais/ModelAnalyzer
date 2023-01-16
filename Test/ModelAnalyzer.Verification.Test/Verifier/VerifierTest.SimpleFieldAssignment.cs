namespace SimpleFieldAssignment.Test;

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
        Verifier TestObject = CreateSimpleFieldAssignmentVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: false, assignmentValue: true, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneBooleanFieldError()
    {
        Verifier TestObject = CreateSimpleFieldAssignmentVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: true, assignmentValue: true, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneBooleanFieldWithWriteError()
    {
        Verifier TestObject = CreateSimpleFieldAssignmentVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: false, assignmentValue: true, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerFieldSuccess()
    {
        Verifier TestObject = CreateSimpleFieldAssignmentVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 0, assignmentValue: 1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerFieldError()
    {
        Verifier TestObject = CreateSimpleFieldAssignmentVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 1, assignmentValue: 1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerFieldWithWriteError()
    {
        Verifier TestObject = CreateSimpleFieldAssignmentVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 0, assignmentValue: 1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointFieldSuccess()
    {
        Verifier TestObject = CreateSimpleFieldAssignmentVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 0.0, assignmentValue: 1.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointFieldError()
    {
        Verifier TestObject = CreateSimpleFieldAssignmentVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 1.0, assignmentValue: 1.0, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointFieldWithWriteError()
    {
        Verifier TestObject = CreateSimpleFieldAssignmentVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 0.0, assignmentValue: 1.0, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private Verifier CreateSimpleFieldAssignmentVerifier<TValue, TExpression>(TValue invariantTestValue, TValue assignmentValue, int maxDepth)
        where TExpression : Expression, ILiteralExpression<TValue>, ILiteralExpression, new()
    {
        TExpression Zero = new() { Value = invariantTestValue };
        TExpression AssignmentSource = new() { Value = assignmentValue };

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

        AssignmentStatement Assignment = new() { DestinationName = TestField.Name, Expression = AssignmentSource };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            AccessModifier = AccessModifier.Public,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            LocalTable = ReadOnlyLocalTable.Empty,
            StatementList = new() { Assignment },
            EnsureList = new(),
            ReturnType = ExpressionType.Void,
        };

        MethodTable MethodTable = new();
        MethodTable.AddItem(TestMethod);

        ClassModel ClassModel = new ClassModel()
        {
            Name = ClassName,
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
            ClassModelTable = new Dictionary<string, ClassModel>() { { ClassName, ClassModel } },
            ClassName = ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = TestFieldTable.AsReadOnly(),
            MethodTable = MethodTable.AsReadOnly(),
            InvariantList = InvariantList,
        };

        return TestObject;
    }
}
