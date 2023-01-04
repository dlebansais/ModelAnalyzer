namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string RequireSourceCodeBoolean1 = @"
using System;

class Program_Verifier_RequireBoolean1
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

class Program_Verifier_RequireBoolean2
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

class Program_Verifier_RequireBoolean3
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

class Program_Verifier_RequireBoolean4
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

class Program_Verifier_RequireBoolean5
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

class Program_Verifier_RequireBoolean6
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

class Program_Verifier_RequireBoolean7
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

class Program_Verifier_RequireBoolean8
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
    public void Verifier_RequireBoolean1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeBoolean1, maxDepth: 0, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireBoolean1_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeBoolean1, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireBoolean2_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeBoolean2, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireBoolean3_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeBoolean3, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireBoolean4_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeBoolean4, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireBoolean5_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeBoolean5, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireBoolean6_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeBoolean6, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireBoolean7_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeBoolean7, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireBoolean8_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeBoolean8, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    private const string RequireSourceCodeInteger1 = @"
using System;

class Program_Verifier_RequireInteger1
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

class Program_Verifier_RequireInteger2
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

class Program_Verifier_RequireInteger3
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

class Program_Verifier_RequireInteger4
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

class Program_Verifier_RequireInteger5
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

class Program_Verifier_RequireInteger6
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

class Program_Verifier_RequireInteger7
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

class Program_Verifier_RequireInteger8
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
    public void Verifier_RequireInteger1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeInteger1, maxDepth: 0, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireInteger1_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeInteger1, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireInteger2_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeInteger2, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireInteger3_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeInteger3, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireInteger4_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeInteger4, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireInteger5_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeInteger5, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireInteger6_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeInteger6, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireInteger7_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeInteger7, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireInteger8_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeInteger8, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    private const string RequireSourceCodeFloatingPoint1 = @"
using System;

class Program_Verifier_RequireFloatingPoint1
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

class Program_Verifier_RequireFloatingPoint2
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

class Program_Verifier_RequireFloatingPoint3
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

class Program_Verifier_RequireFloatingPoint4
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

class Program_Verifier_RequireFloatingPoint5
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

class Program_Verifier_RequireFloatingPoint6
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

class Program_Verifier_RequireFloatingPoint7
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

class Program_Verifier_RequireFloatingPoint8
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
    public void Verifier_RequireFloatingPoint1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint1, maxDepth: 0, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireFloatingPoint1_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint1, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireFloatingPoint2_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint2, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireFloatingPoint3_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint3, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireFloatingPoint4_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint4, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireFloatingPoint5_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint5, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireFloatingPoint6_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint6, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireFloatingPoint7_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint7, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_RequireFloatingPoint8_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(RequireSourceCodeFloatingPoint8, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }
}
