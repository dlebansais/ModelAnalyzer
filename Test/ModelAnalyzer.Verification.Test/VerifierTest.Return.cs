﻿namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string ReturnSourceCodeBoolean1 = @"
using System;

class Program_Verifier_ReturnBoolean1
{
    bool X;

    bool Read()
    {
        return X;
    }
}
";

    private const string ReturnSourceCodeBoolean2 = @"
using System;

class Program_Verifier_ReturnBoolean1
{
    bool X;

    bool Read()
    {
        return X;
    }
    // Ensure: Result != false
}
";

    private const string ReturnSourceCodeBoolean3 = @"
using System;

class Program_Verifier_ReturnBoolean1
{
    bool Read()
    {
        return false;
    }
    // Ensure: Result == false
}
";

    private const string ReturnSourceCodeBoolean4 = @"
using System;

class Program_Verifier_ReturnBoolean1
{
    bool Read()
    {
        return true;
    }
    // Ensure: Result == true
}
";

    private const string ReturnSourceCodeBoolean5 = @"
using System;

class Program_Verifier_ReturnBoolean1
{
    bool X = true;
    bool Y = false;

    bool Read()
    {
        return X && Y;
    }
    // Ensure: Result == Y
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnBoolean1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeBoolean1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnBoolean2_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeBoolean2, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnBoolean3_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeBoolean3, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnBoolean4_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeBoolean4, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnBoolean5_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeBoolean5, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ReturnSourceCodeInteger1 = @"
using System;

class Program_Verifier_ReturnInteger1
{
    int X;

    int Read()
    {
        return X;
    }
}
";

    private const string ReturnSourceCodeInteger2 = @"
using System;

class Program_Verifier_ReturnInteger1
{
    int X;

    int Read()
    {
        return X;
    }
    // Ensure: Result != 0
}
";

    private const string ReturnSourceCodeInteger3 = @"
using System;

class Program_Verifier_ReturnInteger1
{
    int Read()
    {
        return 0;
    }
    // Ensure: Result == 0
}
";

    private const string ReturnSourceCodeInteger4 = @"
using System;

class Program_Verifier_ReturnInteger1
{
    int Read()
    {
        return 1;
    }
    // Ensure: Result == 1
}
";

    private const string ReturnSourceCodeInteger5 = @"
using System;

class Program_Verifier_ReturnInteger1
{
    int X = 1;
    int Y = 0;

    int Read()
    {
        return X + Y;
    }
    // Ensure: Result == X
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnInteger1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeInteger1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnInteger2_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeInteger2, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnInteger3_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeInteger3, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnInteger4_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeInteger4, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnInteger5_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeInteger5, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ReturnSourceCodeFloatingPoint1 = @"
using System;

class Program_Verifier_ReturnFloatingPoint1
{
    double X;

    double Read()
    {
        return X;
    }
}
";

    private const string ReturnSourceCodeFloatingPoint2 = @"
using System;

class Program_Verifier_ReturnFloatingPoint1
{
    double X;

    double Read()
    {
        return X;
    }
    // Ensure: Result != 0.0
}
";

    private const string ReturnSourceCodeFloatingPoint3 = @"
using System;

class Program_Verifier_ReturnFloatingPoint1
{
    double Read()
    {
        return 0.0;
    }
    // Ensure: Result == 0.0
}
";

    private const string ReturnSourceCodeFloatingPoint4 = @"
using System;

class Program_Verifier_ReturnFloatingPoint1
{
    double Read()
    {
        return 1.0;
    }
    // Ensure: Result == 1.0
}
";

    private const string ReturnSourceCodeFloatingPoint5 = @"
using System;

class Program_Verifier_ReturnFloatingPoint1
{
    double X = 1.0;
    double Y = 0.0;

    double Read()
    {
        return X + Y;
    }
    // Ensure: Result == X
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnFloatingPoint1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnFloatingPoint2_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint2, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnFloatingPoint3_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint3, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnFloatingPoint4_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint4, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_ReturnFloatingPoint5_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint5, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }
}