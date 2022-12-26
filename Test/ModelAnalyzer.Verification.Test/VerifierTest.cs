namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
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
}
