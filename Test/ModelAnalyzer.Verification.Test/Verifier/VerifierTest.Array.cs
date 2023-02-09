namespace Array.Test;

using System;
using ModelAnalyzer;
using NUnit.Framework;
using Verification.Test;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string ArraySourceCodeBoolean1 = @"
using System;

class Program_Verifier_Boolean1
{
    public bool Read()
    {
        bool[] X = new bool[1];

        return X[0];
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ArraySourceCodeBoolean1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ArraySourceCodeInteger1 = @"
using System;

class Program_Verifier_Integer1
{
    public int Read()
    {
        int[] X = new int[1];

        return X[0];
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ArraySourceCodeInteger1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ArraySourceCodeFloatingPoint1 = @"
using System;

class Program_Verifier_FloatingPoint1
{
    public double Read()
    {
        double[] X = new double[1];

        return X[0];
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ArraySourceCodeFloatingPoint1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ArraySourceCodeInteger2 = @"
using System;

class Program_Verifier_Integer2
{
    public bool Read()
    {
        int[] X = new int[1];

        return X == new int[1];
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer2_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ArraySourceCodeInteger2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ArraySourceCodeInteger3 = @"
using System;

class Program_Verifier_Integer3_1
{
    public int[] Y { get; set; } = new int[1];
}

class Program_Verifier_Integer3_2
{
    public int Read()
    {
        Program_Verifier_Integer3_1 X = new();

        return X.Y[0];
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer3_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ArraySourceCodeInteger3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }
}
