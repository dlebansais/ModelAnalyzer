namespace MethodCall.Test;

using System;
using ModelAnalyzer;
using NUnit.Framework;
using Verification.Test;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string MethodCallSourceCodeInteger1 = @"
using System;

class Program_Verifier_Integer1
{
    public void Write1()
    {
        Write2();
    }

    public void Write2()
    {
    }
}
";

    private const string MethodCallSourceCodeInteger2 = @"
using System;

class Program_Verifier_Integer2
{
    int X;

    public void Write1(int x)
    {
        Write2(x);
    }

    void Write2(int x)
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string MethodCallSourceCodeInteger3 = @"
using System;

class Program_Verifier_Integer3
{
    int X;
    int Y;

    public void Write1(int y)
    {
        Write2(y);
    }

    void Write2(int y)
    {
        Write3(y);
    }

    void Write3(int y)
    {
        Y = y;
    }
}
// Invariant: Y == 0
";

    private const string MethodCallSourceCodeInteger4 = @"
using System;

class Program_Verifier_Integer4
{
    int X;

    public void Write1(int x)
    {
        Write2(x);
    }

    void Write2(int x)
    // Require: x == 0
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string MethodCallSourceCodeInteger5 = @"
using System;

class Program_Verifier_Integer5
{
    int X;

    public void Write1(int x)
    {
        Write2(0);
    }

    void Write2(int x)
    // Require: x == 0
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string MethodCallSourceCodeInteger6 = @"
using System;

class Program_Verifier_Integer6
{
    public int X { get; set; }

    public void Write1(int x)
    {
        X = x;
        Write2(0);
    }

    void Write2(int x)
    {
        X = x;
    }
    // Ensure: X == 0
}
// Invariant: X == 0
";

    private const string MethodCallSourceCodeInteger7 = @"
using System;

class Program_Verifier_Integer7
{
    public int X { get; set; }

    public int Write1(int x)
    {
        Write2(1);
        return X;
    }
    // Ensure: X == 0

    void Write2(int x)
    {
        X = x;
    }
}
";

    private const string MethodCallSourceCodeInteger8 = @"
using System;

class Program_Verifier_Integer8
{
    public int X { get; set; }

    public int Write1(int x)
    {
        Write2(0);
        return X;
    }
    // Ensure: X == 0

    void Write2(int x)
    {
        X = x;
    }
}
";

    private const string MethodCallSourceCodeInteger9 = @"
using System;

class Program_Verifier_Integer9
{
    int X;

    public void Write1(int x)
    {
        Write2(x);
    }

    void Write2(int x)
    {
        Write3(x);
    }

    void Write3(int x)
    // Require: x == 0
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string MethodCallSourceCodeInteger10 = @"
using System;

class Program_Verifier_Integer10
{
    public int X { get; set; }

    public void Write1(int x)
    {
        Write2(x);
    }
    // Ensure: X == 1

    void Write2(int x)
    {
        Write3(x);
    }
    // Ensure: X == 0

    void Write3(int x)
    {
        X = x;
    }
}
// Invariant: X == 0
";

    private const string MethodCallSourceCodeInteger11 = @"
using System;

class Program_Verifier_Integer11
{
    public int X { get; set; }

    public void Write1(int x)
    {
        X = x;
    }
    // Ensure: X == 0 && X != 0
}
";

    private const string MethodCallSourceCodeInteger12 = @"
using System;

class Program_Verifier_Integer12
{
    public int X { get; set; }

    public void Write1(int x)
    {
        Write2(x);
    }

    void Write2(int x)
    {
        X = x;
    }
    // Ensure: X == 0 && X != 0
}
";

    private const string MethodCallSourceCodeInteger13 = @"
using System;

class Program_Verifier_Integer13
{
    int X;

    public void Write1(int x)
    {
        Write2(Write3(x));
    }

    void Write2(int x)
    {
        X = x;
    }

    int Write3(int x)
    // Require: x == 1
    {
        return x;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer2_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer3_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer4_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer5_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer6_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer7_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger7, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer8_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger8, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer9_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger9, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer10_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger10, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer11_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger11, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer12_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger12, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer13_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger13, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }
}
