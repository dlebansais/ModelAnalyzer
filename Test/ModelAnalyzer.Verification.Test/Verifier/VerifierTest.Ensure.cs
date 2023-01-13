namespace Ensure.Test;

using System;
using ModelAnalyzer;
using NUnit.Framework;
using Verification.Test;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string EnsureSourceCodeBoolean1 = @"
using System;

class Program_Verifier_Boolean1
{
    public bool X { get; set; }

    public void Write1(bool x)
    {
        X = x;
    }

    public void Write2(bool x)
    {
        X = x;
    }
    // Ensure: X == false
}
";

    private const string EnsureSourceCodeBoolean2 = @"
using System;

class Program_Verifier_Boolean2
{
    bool X;

    public void Write(bool x)
    {
        X = x;
    }
}
";

    private const string EnsureSourceCodeBoolean3 = @"
using System;

class Program_Verifier_Boolean3
{
    public bool X { get; set; }

    public void Write(bool x)
    // Require: x == false || x == true
    {
        X = x;
    }
    // Ensure: X == false
}
";

    private const string EnsureSourceCodeBoolean4 = @"
using System;

class Program_Verifier_Boolean4
{
    public bool X { get; set; }

    public void Write(bool x)
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

class Program_Verifier_Boolean5
{
    public bool X { get; set; }
    public bool Y { get; set; }

    public void Write(bool x, bool y)
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

class Program_Verifier_Boolean6
{
    public bool X { get; set; }
    public bool Y { get; set; }

    public void Write(bool x, bool y)
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

class Program_Verifier_Boolean7
{
    public bool X { get; set; }
    public bool Y { get; set; }

    public void Write(bool x, bool y)
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
    public void Verifier_Boolean1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeBoolean1, maxDepth: 0, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean1_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeBoolean1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean2_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeBoolean2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean3_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeBoolean3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean4_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeBoolean4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean5_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeBoolean5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean6_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeBoolean6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean7_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeBoolean7, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string EnsureSourceCodeInteger1 = @"
using System;

class Program_Verifier_Integer1
{
    public int X { get; set; }

    public void Write1(int x)
    {
        X = x;
    }

    public void Write2(int x)
    {
        X = x;
    }
    // Ensure: X == 0
}
";

    private const string EnsureSourceCodeInteger2 = @"
using System;

class Program_Verifier_Integer2
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}
";

    private const string EnsureSourceCodeInteger3 = @"
using System;

class Program_Verifier_Integer3
{
    public int X { get; set; }

    public void Write(int x)
    // Require: x == 0
    {
        X = x;
    }
    // Ensure: X >= 0
}
";

    private const string EnsureSourceCodeInteger4 = @"
using System;

class Program_Verifier_Integer4
{
    public int X { get; set; }

    public void Write(int x)
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

class Program_Verifier_Integer5
{
    public int X { get; set; }
    public int Y { get; set; }

    public void Write(int x, int y)
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

class Program_Verifier_Integer6
{
    public int X { get; set; }
    public int Y { get; set; }

    public void Write(int x, int y)
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

class Program_Verifier_Integer7
{
    public int X { get; set; }
    public int Y { get; set; }

    public void Write(int x, int y)
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
    public void Verifier_Integer1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeInteger1, maxDepth: 0, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer1_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeInteger1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer2_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeInteger2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer3_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeInteger3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer4_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeInteger4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer5_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeInteger5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer6_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeInteger6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer7_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeInteger7, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string EnsureSourceCodeFloatingPoint1 = @"
using System;

class Program_Verifier_FloatingPoint1
{
    public double X { get; set; }

    public void Write1(double x)
    {
        X = x;
    }

    public void Write2(double x)
    {
        X = x;
    }
    // Ensure: X == 0
}
";

    private const string EnsureSourceCodeFloatingPoint2 = @"
using System;

class Program_Verifier_FloatingPoint2
{
    double X;

    public void Write(double x)
    {
        X = x;
    }
}
";

    private const string EnsureSourceCodeFloatingPoint3 = @"
using System;

class Program_Verifier_FloatingPoint3
{
    public double X { get; set; }

    public void Write(double x)
    // Require: x == 0
    {
        X = x;
    }
    // Ensure: X >= 0
}
";

    private const string EnsureSourceCodeFloatingPoint4 = @"
using System;

class Program_Verifier_FloatingPoint4
{
    public double X { get; set; }

    public void Write(double x)
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

class Program_Verifier_FloatingPoint5
{
    public double X { get; set; }
    public double Y { get; set; }

    public void Write(double x, double y)
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

class Program_Verifier_FloatingPoint6
{
    public double X { get; set; }
    public double Y { get; set; }

    public void Write(double x, double y)
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

class Program_Verifier_FloatingPoint7
{
    public double X { get; set; }
    public double Y { get; set; }

    public void Write(double x, double y)
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
    public void Verifier_FloatingPoint1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint1, maxDepth: 0, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint1_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint2_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint3_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint4_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint5_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint6_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint7_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(EnsureSourceCodeFloatingPoint7, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }
}
