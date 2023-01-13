namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string NewObjectSourceCode1 = @"
using System;

class Program_Verifier_NewObject1_1
{
    public int X { get; set; }
}

class Program_Verifier_NewObject1_2
{
    public Program_Verifier_NewObject1_1? X { get; set; } = null;
    public Program_Verifier_NewObject1_1 Y { get; set; } = new();

    public void Write()
    {
        X = new Program_Verifier_NewObject1_1();
        int Z = 0;

        if (Z == 0)
        {
            X = new Program_Verifier_NewObject1_1();
        }
        else
        {
            Y = new Program_Verifier_NewObject1_1();
        }
    }
    // Ensure: X == null
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_NewObject1_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(NewObjectSourceCode1, maxDepth: 2, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    private const string NewObjectSourceCode2 = @"
using System;

class Program_Verifier_NewObject2_1
{
    public int X { get; set; }
}

class Program_Verifier_NewObject2_2
{
    public Program_Verifier_NewObject2_1 X { get; set; } = new();

    public Program_Verifier_NewObject2_1 Write(Program_Verifier_NewObject2_1 x)
    // Require: x == X
    {
        return x;
    }
    // Ensure: Result == X
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_NewObject2_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(NewObjectSourceCode2, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string NewObjectSourceCode3 = @"
using System;

class Program_Verifier_NewObject3_1
{
    public int X { get; set; }
}

class Program_Verifier_NewObject3_2
{
    public Program_Verifier_NewObject3_1 X { get; set; } = new();

    public Program_Verifier_NewObject3_1 Write(Program_Verifier_NewObject3_1 x)
    {
        return x;
    }
    // Ensure: Result == X
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_NewObject3_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(NewObjectSourceCode3, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }
}
