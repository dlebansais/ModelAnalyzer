namespace ConditionalAssignment.Test;

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
        Verifier TestObject = CreateConditionalAssignmentVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: false, assignmentValue: true, maxDepth: 0, whenTrue: true);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneBooleanFieldError()
    {
        Verifier TestObject = CreateConditionalAssignmentVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: true, assignmentValue: true, maxDepth: 0, whenTrue: true);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneBooleanFieldWithWriteSuccess()
    {
        Verifier TestObject = CreateConditionalAssignmentVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: false, assignmentValue: true, maxDepth: 1, whenTrue: false);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneBooleanFieldWithWriteError()
    {
        Verifier TestObject = CreateConditionalAssignmentVerifier<bool, LiteralBooleanValueExpression>(invariantTestValue: false, assignmentValue: true, maxDepth: 1, whenTrue: true);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerFieldSuccess()
    {
        Verifier TestObject = CreateConditionalAssignmentVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 0, assignmentValue: 1, maxDepth: 0, whenTrue: true);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerFieldError()
    {
        Verifier TestObject = CreateConditionalAssignmentVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 1, assignmentValue: 1, maxDepth: 0, whenTrue: true);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerFieldWithWriteSuccess()
    {
        Verifier TestObject = CreateConditionalAssignmentVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 0, assignmentValue: 1, maxDepth: 1, whenTrue: false);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneIntegerFieldWithWriteError()
    {
        Verifier TestObject = CreateConditionalAssignmentVerifier<int, LiteralIntegerValueExpression>(invariantTestValue: 0, assignmentValue: 1, maxDepth: 1, whenTrue: true);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointFieldSuccess()
    {
        Verifier TestObject = CreateConditionalAssignmentVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 0.0, assignmentValue: 1.0, maxDepth: 0, whenTrue: true);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointFieldError()
    {
        Verifier TestObject = CreateConditionalAssignmentVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 1.0, assignmentValue: 1.0, maxDepth: 0, whenTrue: true);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointFieldWithWriteSuccess()
    {
        Verifier TestObject = CreateConditionalAssignmentVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 0.0, assignmentValue: 1.0, maxDepth: 1, whenTrue: false);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_OneFloatingPointFieldWithWriteError()
    {
        Verifier TestObject = CreateConditionalAssignmentVerifier<double, LiteralFloatingPointValueExpression>(invariantTestValue: 0.0, assignmentValue: 1.0, maxDepth: 1, whenTrue: true);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private Verifier CreateConditionalAssignmentVerifier<TValue, TExpression>(TValue invariantTestValue, TValue assignmentValue, int maxDepth, bool whenTrue)
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
        ConditionalStatement Conditional = new()
        {
            Condition = VariableEqualZero,
            WhenTrueStatementList = whenTrue ? new List<Statement>() { Assignment } : new List<Statement>(),
            WhenFalseStatementList = whenTrue ? new List<Statement>() : new List<Statement>() { Assignment },
        };

        string TestMethodName = "Write";
        MethodName MethodName = new() { Text = TestMethodName };
        Method TestMethod = new()
        {
            Name = MethodName,
            AccessModifier = AccessModifier.Public,
            ParameterTable = ReadOnlyParameterTable.Empty,
            RequireList = new(),
            LocalTable = ReadOnlyLocalTable.Empty,
            StatementList = new() { Conditional },
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
