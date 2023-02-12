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
    private const string ForLoopSourceCodeBoolean1 = @"
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
    public void Verifier_Boolean1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ForLoopSourceCodeBoolean1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }
}
