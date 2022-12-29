﻿namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string EnsureSourceCodeBoolean1 = @"
using System;

class Program_Verifier_EnsureBoolean1
{
    bool X;

    void Write1(bool x)
    {
        X = x;
    }

    void Write2(bool x)
    {
        X = x;
    }
    // Ensure: X == false
}
";

    private const string EnsureSourceCodeBoolean2 = @"
using System;

class Program_Verifier_EnsureBoolean2
{
    bool X;

    void Write(bool x)
    {
        X = x;
    }
}
";

    private const string EnsureSourceCodeBoolean3 = @"
using System;

class Program_Verifier_EnsureBoolean3
{
    bool X;

    void Write(bool x)
    // Require: x == false || x == true
    {
        X = x;
    }
    // Ensure: X == false
}
";

    private const string EnsureSourceCodeBoolean4 = @"
using System;

class Program_Verifier_EnsureBoolean4
{
    bool X;

    void Write(bool x)
    // Require: x == false
    {
        X = x;
    }
    // Ensure: X == false
    // Ensure: X != true
}
";

    private const string EnsureSourceCodeBoolean5 = @"
using System;

class Program_Verifier_EnsureBoolean5
{
    bool X;
    bool Y;

    void Write(bool x, bool y)
    // Require: x == false
    {
        X = x;
        Y = y;
    }
    // Ensure: X == false
    // Ensure: Y == false
}
";

    private const string EnsureSourceCodeBoolean6 = @"
using System;

class Program_Verifier_EnsureBoolean6
{
    bool X;
    bool Y;

    void Write(bool x, bool y)
    // Require: y == false
    {
        X = x;
        Y = y;
    }
    // Ensure: X == false
    // Ensure: Y == false
}
";

    private const string EnsureSourceCodeBoolean7 = @"
using System;

class Program_Verifier_EnsureBoolean7
{
    bool X;
    bool Y;

    void Write(bool x, bool y)
    // Require: x == false
    // Require: y == false
    {
        X = x;
        Y = y;
    }
    // Ensure: X == false
    // Ensure: Y == false
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureBoolean1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeBoolean1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureBoolean1_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeBoolean1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureBoolean2_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeBoolean2, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureBoolean3_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeBoolean3, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureBoolean4_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeBoolean4, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureBoolean5_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeBoolean5, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureBoolean6_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeBoolean6, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureBoolean7_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeBoolean7, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string EnsureSourceCodeInteger1 = @"
using System;

class Program_Verifier_EnsureInteger1
{
    int X;

    void Write1(int x)
    {
        X = x;
    }

    void Write2(int x)
    {
        X = x;
    }
    // Ensure: X == 0
}
";

    private const string EnsureSourceCodeInteger2 = @"
using System;

class Program_Verifier_EnsureInteger2
{
    int X;

    void Write(int x)
    {
        X = x;
    }
}
";

    private const string EnsureSourceCodeInteger3 = @"
using System;

class Program_Verifier_EnsureInteger3
{
    int X;

    void Write(int x)
    // Require: x == 0
    {
        X = x;
    }
    // Ensure: X >= 0
}
";

    private const string EnsureSourceCodeInteger4 = @"
using System;

class Program_Verifier_EnsureInteger4
{
    int X;

    void Write(int x)
    // Require: x == 0
    {
        X = x;
    }
    // Ensure: X >= 0
    // Ensure: X <= 0
}
";

    private const string EnsureSourceCodeInteger5 = @"
using System;

class Program_Verifier_EnsureInteger5
{
    int X;
    int Y;

    void Write(int x, int y)
    // Require: x == 0
    {
        X = x;
        Y = y;
    }
    // Ensure: X == 0
    // Ensure: Y == 0
}
";

    private const string EnsureSourceCodeInteger6 = @"
using System;

class Program_Verifier_EnsureInteger6
{
    int X;
    int Y;

    void Write(int x, int y)
    // Require: y == 0
    {
        X = x;
        Y = y;
    }
    // Ensure: X == 0
    // Ensure: Y == 0
}
";

    private const string EnsureSourceCodeInteger7 = @"
using System;

class Program_Verifier_EnsureInteger7
{
    int X;
    int Y;

    void Write(int x, int y)
    // Require: x == 0
    // Require: y == 0
    {
        X = x;
        Y = y;
    }
    // Ensure: X == 0
    // Ensure: Y == 0
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureInteger1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeInteger1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureInteger1_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeInteger1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureInteger2_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeInteger2, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureInteger3_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeInteger3, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureInteger4_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeInteger4, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureInteger5_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeInteger5, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureInteger6_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeInteger6, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureInteger7_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeInteger7, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string EnsureSourceCodeFloatingPoint1 = @"
using System;

class Program_Verifier_EnsureFloatingPoint1
{
    double X;

    void Write1(double x)
    {
        X = x;
    }

    void Write2(double x)
    {
        X = x;
    }
    // Ensure: X == 0
}
";

    private const string EnsureSourceCodeFloatingPoint2 = @"
using System;

class Program_Verifier_EnsureFloatingPoint2
{
    double X;

    void Write(double x)
    {
        X = x;
    }
}
";

    private const string EnsureSourceCodeFloatingPoint3 = @"
using System;

class Program_Verifier_EnsureFloatingPoint3
{
    double X;

    void Write(double x)
    // Require: x == 0
    {
        X = x;
    }
    // Ensure: X >= 0
}
";

    private const string EnsureSourceCodeFloatingPoint4 = @"
using System;

class Program_Verifier_EnsureFloatingPoint4
{
    double X;

    void Write(double x)
    // Require: x == 0
    {
        X = x;
    }
    // Ensure: X >= 0
    // Ensure: X <= 0
}
";

    private const string EnsureSourceCodeFloatingPoint5 = @"
using System;

class Program_Verifier_EnsureFloatingPoint5
{
    double X;
    double Y;

    void Write(double x, double y)
    // Require: x == 0
    {
        X = x;
        Y = y;
    }
    // Ensure: X == 0
    // Ensure: Y == 0
}
";

    private const string EnsureSourceCodeFloatingPoint6 = @"
using System;

class Program_Verifier_EnsureFloatingPoint6
{
    double X;
    double Y;

    void Write(double x, double y)
    // Require: y == 0
    {
        X = x;
        Y = y;
    }
    // Ensure: X == 0
    // Ensure: Y == 0
}
";

    private const string EnsureSourceCodeFloatingPoint7 = @"
using System;

class Program_Verifier_EnsureFloatingPoint7
{
    double X;
    double Y;

    void Write(double x, double y)
    // Require: x == 0
    // Require: y == 0
    {
        X = x;
        Y = y;
    }
    // Ensure: X == 0
    // Ensure: Y == 0
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureFloatingPoint1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint1, maxDepth: 0);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureFloatingPoint1_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureFloatingPoint2_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint2, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureFloatingPoint3_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint3, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureFloatingPoint4_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint4, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureFloatingPoint5_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint5, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureFloatingPoint6_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint6, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_EnsureFloatingPoint7_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint7, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }
}
