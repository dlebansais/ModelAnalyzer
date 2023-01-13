namespace Require.Test;

using System;
using ModelAnalyzer;
using NUnit.Framework;
using Verification.Test;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string RequireSourceCodeBoolean1 = @"
using System;

class Program_Verifier_Boolean1
{
    bool X;

    public void Write(bool x)
    {
        X = x;
    }
}
// Invariant: X == false
";

    private const string RequireSourceCodeBoolean2 = @"
using System;

class Program_Verifier_Boolean2
{
    bool X;

    public void Write(bool x)
    // Require: x == false
    {
        X = x;
    }
}
// Invariant: X == false
";

    private const string RequireSourceCodeBoolean3 = @"
using System;

class Program_Verifier_Boolean3
{
    bool X;

    public void Write(bool x)
    // Require: x == false || x == true
    {
        X = x;
    }
}
// Invariant: X == false
";

    private const string RequireSourceCodeBoolean4 = @"
using System;

class Program_Verifier_Boolean4
{
    bool X;

    public void Write(bool x)
    // Require: x != true
    // Require: x == false
    {
        X = x;
    }
}
// Invariant: X == false
";

    private const string RequireSourceCodeBoolean5 = @"
using System;

class Program_Verifier_Boolean5
{
    bool Z;

    public void Write(bool x, bool y)
    // Require: x == false
    {
        Z = x || y;
    }
}
// Invariant: Z == false
";

    private const string RequireSourceCodeBoolean6 = @"
using System;

class Program_Verifier_Boolean6
{
    bool Z;

    public void Write(bool x, bool y)
    // Require: y == false
    {
        Z = x || y;
    }
}
// Invariant: Z == false
";

    private const string RequireSourceCodeBoolean7 = @"
using System;

class Program_Verifier_Boolean7
{
    bool Z;

    public void Write(bool x, bool y)
    // Require: x == false
    // Require: y == false
    {
        Z = x || y;
    }
}
// Invariant: Z == false
";

    private const string RequireSourceCodeBoolean8 = @"
using System;

class Program_Verifier_Boolean8
{
    bool X;

    public void Write(bool x)
    // Require: x == false
    // Require: x != false
    {
        X = x;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeBoolean1, maxDepth: 0, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean1_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeBoolean1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean2_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeBoolean2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean3_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeBoolean3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean4_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeBoolean4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean5_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeBoolean5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean6_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeBoolean6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean7_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeBoolean7, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean8_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeBoolean8, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    private const string RequireSourceCodeInteger1 = @"
using System;

class Program_Verifier_Integer1
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string RequireSourceCodeInteger2 = @"
using System;

class Program_Verifier_Integer2
{
    int X;

    public void Write(int x)
    // Require: x == 0
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string RequireSourceCodeInteger3 = @"
using System;

class Program_Verifier_Integer3
{
    int X;

    public void Write(int x)
    // Require: x >= 0
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string RequireSourceCodeInteger4 = @"
using System;

class Program_Verifier_Integer4
{
    int X;

    public void Write(int x)
    // Require: x >= 0
    // Require: x <= 0
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string RequireSourceCodeInteger5 = @"
using System;

class Program_Verifier_Integer5
{
    int Z;

    public void Write(int x, int y)
    // Require: x == 0
    {
        Z = x + y;
    }
}
// Invariant: Z == 0
";

    private const string RequireSourceCodeInteger6 = @"
using System;

class Program_Verifier_Integer6
{
    int Z;

    public void Write(int x, int y)
    // Require: y == 0
    {
        Z = x + y;
    }
}
// Invariant: Z == 0
";

    private const string RequireSourceCodeInteger7 = @"
using System;

class Program_Verifier_Integer7
{
    int Z;

    public void Write(int x, int y)
    // Require: x == 0
    // Require: y == 0
    {
        Z = x + y;
    }
}
// Invariant: Z == 0
";

    private const string RequireSourceCodeInteger8 = @"
using System;

class Program_Verifier_Integer8
{
    int X;

    public void Write(int x)
    // Require: x == 0
    // Require: x != 0
    {
        X = x;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeInteger1, maxDepth: 0, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer1_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeInteger1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer2_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeInteger2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer3_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeInteger3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer4_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeInteger4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer5_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeInteger5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer6_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeInteger6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer7_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeInteger7, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer8_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeInteger8, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    private const string RequireSourceCodeFloatingPoint1 = @"
using System;

class Program_Verifier_FloatingPoint1
{
    double X;

    public void Write(double x)
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string RequireSourceCodeFloatingPoint2 = @"
using System;

class Program_Verifier_FloatingPoint2
{
    double X;

    public void Write(double x)
    // Require: x == 0
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string RequireSourceCodeFloatingPoint3 = @"
using System;

class Program_Verifier_FloatingPoint3
{
    double X;

    public void Write(double x)
    // Require: x >= 0
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string RequireSourceCodeFloatingPoint4 = @"
using System;

class Program_Verifier_FloatingPoint4
{
    double X;

    public void Write(double x)
    // Require: x >= 0
    // Require: x <= 0
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string RequireSourceCodeFloatingPoint5 = @"
using System;

class Program_Verifier_FloatingPoint5
{
    double Z;

    public void Write(double x, double y)
    // Require: x == 0
    {
        Z = x + y;
    }
}
// Invariant: Z == 0
";

    private const string RequireSourceCodeFloatingPoint6 = @"
using System;

class Program_Verifier_FloatingPoint6
{
    double Z;

    public void Write(double x, double y)
    // Require: y == 0
    {
        Z = x + y;
    }
}
// Invariant: Z == 0
";

    private const string RequireSourceCodeFloatingPoint7 = @"
using System;

class Program_Verifier_FloatingPoint7
{
    double Z;

    public void Write(double x, double y)
    // Require: x == 0
    // Require: y == 0
    {
        Z = x + y;
    }
}
// Invariant: Z == 0
";

    private const string RequireSourceCodeFloatingPoint8 = @"
using System;

class Program_Verifier_FloatingPoint8
{
    double X;

    public void Write(double x)
    // Require: x == 0.0
    // Require: x != 0.0
    {
        X = x;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint1, maxDepth: 0, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint1_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint2_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint3_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint4_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint5_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint6_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint7_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint7, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint8_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint8, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }
}
