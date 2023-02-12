namespace ForLoop.Test;

using System;
using ModelAnalyzer;
using NUnit.Framework;
using Verification.Test;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string ForLoopSourceCode1 = @"
using System;

class Program_Verifier_Loop1
{
    public double[] Read()
    {
        double[] X = new double[10];

        for (int i = 0; i < X.Length; i++)
        {
            X[i] = i * 1.5;
        }

        return X;
    }
    // TODO Ensure: Result[2] == 3
    // TODO Ensure: ∀i Result[i] == i * 1.5
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_ForLoop1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ForLoopSourceCode1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ForLoopSourceCode2 = @"
using System;

class Program_Verifier_Loop1
{
    public double[] Read()
    {
        double[] X = new double[10];

        for (int i = 0; (10 / i) == 0; i++)
        {
            X[i] = i * 1.5;
        }

        return X;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_ForLoop2_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ForLoopSourceCode2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.False);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.AssumeError));
    }
}
