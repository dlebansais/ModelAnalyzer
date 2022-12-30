namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string MethodCallSourceCodeInteger1 = @"
using System;

class Program_Verifier_MethodCallInteger1
{
    public void Write1()
    {
        Write2();
    }

    public void Write2()
    {
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }
}
