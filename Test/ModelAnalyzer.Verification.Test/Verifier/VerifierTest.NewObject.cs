namespace NewObject.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ModelAnalyzer;
using NUnit.Framework;
using Verification.Test;

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
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(NewObjectSourceCode1, maxDepth: 2, maxDuration: TimeSpan.MaxValue);

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
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(NewObjectSourceCode2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

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
    public void Verifier_NewObject3_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(NewObjectSourceCode3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    private const string NewObjectSourceCode4 = @"
using System;

class Program_Verifier_NewObject4_1
{
    public int Z { get; set; } = 1;

    public int Read()
    {
        return Z;
    }
    // Ensure: Result == 1
}

class Program_Verifier_NewObject4_2
{
    public Program_Verifier_NewObject4_1 Y { get; set; } = new();

    public int Read()
    {
        return Y.Z;
    }
    // Ensure: Result == 1
}

class Program_Verifier_NewObject4_3
{
    public int Read()
    {
        Program_Verifier_NewObject4_2 X = new();

        return X.Y.Z;
    }
    // Ensure: Result == 1
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_NewObject4_Success()
    {
        List<Verifier> TestObjectList = Tools.CreateMultiVerifierFromSourceCode(NewObjectSourceCode4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        foreach (Verifier Item in TestObjectList)
        {
            Item.Verify();

            VerificationResult VerificationResult = Item.VerificationResult;
            Assert.That(VerificationResult.IsSuccess, Is.True);
        }
    }

    private const string NewObjectSourceCode5 = @"
using System;

class Program_Verifier_NewObject5_1
{
    public int Z { get; set; }
}

class Program_Verifier_NewObject5_2
{
    public Program_Verifier_NewObject5_1 Y { get; set; } = new();
}

class Program_Verifier_NewObject5_3
{
    public int Read()
    {
        Program_Verifier_NewObject5_2 X = new();
        return X.Y.Z;
    }
    // Ensure: Result == 1
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_NewObject5_Error()
    {
        List<Verifier> TestObjectList = Tools.CreateMultiVerifierFromSourceCode(NewObjectSourceCode5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        Assert.That(TestObjectList.Count, Is.EqualTo(3));

        Verifier Verifier0 = TestObjectList[0];
        Verifier Verifier1 = TestObjectList[1];
        Verifier Verifier2 = TestObjectList[2];

        Verifier0.Verify();

        VerificationResult VerificationResult0 = Verifier0.VerificationResult;
        Assert.That(VerificationResult0.IsSuccess, Is.True);

        Verifier1.Verify();

        VerificationResult VerificationResult1 = Verifier1.VerificationResult;
        Assert.That(VerificationResult1.IsSuccess, Is.True);

        Verifier2.Verify();

        VerificationResult VerificationResult2 = Verifier2.VerificationResult;
        Assert.That(VerificationResult2.IsError, Is.True);
        Assert.That(VerificationResult2.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }
}
