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

    private const string ArraySourceCodeInteger4 = @"
using System;

class Program_Verifier_Integer4
{
    public double Read()
    {
        double[] X = new double[1];
        double[] Y = new double[1];

        X[0] = 1.0;
        Y[0] = 2.0;

        return X[0] + Y[0];
    }
    // Ensure: Result == 3.0
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer4_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ArraySourceCodeInteger4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ArraySourceCodeInteger5 = @"
using System;

class Program_Verifier_Integer5
{
    public void Write()
    {
        int[] X;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer5_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ArraySourceCodeInteger5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ArraySourceCodeInteger6 = @"
using System;

class Program_Verifier_Integer6
{
    public int Read()
    {
        double[] X = new double[10];

        return X.Length;
    }
    // Ensure: Result == 10
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer6_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ArraySourceCodeInteger6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ArraySourceCodeInteger7 = @"
using System;

class Program_Verifier_Integer7
{
    public int Read()
    {
        double[] X = new double[10];

        return X.Length;
    }
    // Ensure: Result == 0
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer7_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ArraySourceCodeInteger7, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.False);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    private const string ArraySourceCodeInteger8 = @"
using System;

class Program_Verifier_Integer8
{
    public int Read()
    {
        double[] X = new double[10];

        return LengthOf(X);
    }
    // Ensure: Result == 10

    public int LengthOf(double[] x)
    {
        return x.Length;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer8_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ArraySourceCodeInteger8, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }
}
