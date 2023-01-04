namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string MethodCallSourceCodeInteger1 = @"
using System;

class Program_Verifier_MethodCallInteger1
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

class Program_Verifier_MethodCallInteger2
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

class Program_Verifier_MethodCallInteger3
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

class Program_Verifier_MethodCallInteger4
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

class Program_Verifier_MethodCallInteger5
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

class Program_Verifier_MethodCallInteger6
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

class Program_Verifier_MethodCallInteger7
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

class Program_Verifier_MethodCallInteger8
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

class Program_Verifier_MethodCallInteger9
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

class Program_Verifier_MethodCallInteger10
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

class Program_Verifier_MethodCallInteger11
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

class Program_Verifier_MethodCallInteger12
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

class Program_Verifier_MethodCallInteger13
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
    public void Verifier_MethodCallInteger1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger1, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger2_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger2, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger3_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger3, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger4_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger4, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger5_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger5, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger6_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger6, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger7_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger7, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger8_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger8, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger9_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger9, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger10_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger10, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger11_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger11, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger12_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger12, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MethodCallInteger13_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MethodCallSourceCodeInteger13, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }
}
