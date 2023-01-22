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

    [Test]
    [Category("Verification")]
    public void Verifier_Integer1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

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

    [Test]
    [Category("Verification")]
    public void Verifier_Integer5_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

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

    [Test]
    [Category("Verification")]
    public void Verifier_Integer6_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

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

    [Test]
    [Category("Verification")]
    public void Verifier_Integer8_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger8, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

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
    public void Verifier_Integer13_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger13, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    private const string MethodCallSourceCodeInteger14 = @"
using System;

class Program_Verifier_Integer14_1
{
    public int Y { get; set; }

    public void Other()
    {
    }

    public void Write(int n)
    // Require: n > 0
    {
        Y = n;
    }
    // Ensure: Y > 0
}

class Program_Verifier_Integer14_2
{
    public Program_Verifier_Integer14_1 X { get; set; } = new();

    public void Write1(int n)
    // Require: n > 0
    {
        X.Write(n);
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer14_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger14, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string MethodCallSourceCodeInteger15 = @"
using System;

class Program_Verifier_Integer15_1
{
    public int Z { get; set; }

    public void Write(int n)
    // Require: n == 0
    {
        Z = n + 1;
    }
    // Ensure: Z == n + 1
}

class Program_Verifier_Integer15_2
{
    Program_Verifier_Integer15_1 X = new();
    public int Y { get; set; }

    public void Write1(int n)
    // Require: n != 0
    {
        X.Write(n);
        Y = X.Z;
    }
    // Ensure: Y != 1

    public void Write2(int n)
    // Require: n == -1
    {
        X.Write(n);
        Y = X.Z;
    }
    // Ensure: Y == 0
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer15_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger15, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    private const string MethodCallSourceCodeInteger16 = @"
using System;

class Program_Verifier_Integer16_1
{
    public int Z { get; set; }

    public void Write(int n)
    // Require: n < 0
    {
        Z = n;
    }
    // Ensure: Z >= 0
    // Ensure: Z < 0
}

class Program_Verifier_Integer16_2
{
    Program_Verifier_Integer16_1 X = new();

    public void WriteX(int n)
    // Require: n < 0
    {
        X.Write(n);
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer16_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger16, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    private const string MethodCallSourceCodeInteger17 = @"
using System;

class Program_Verifier_Integer17_1
{
    public int Z { get; set; }

    public void Write2(int x)
    // Require: x == 0
    {
        Z = Write3(x);
    }

    int Write3(int x)
    // Require: x == 1
    {
        return x;
    }
}

class Program_Verifier_Integer17_2
{
    Program_Verifier_Integer17_1 X = new();
    
    public void Write1(int x)
    // Require: x == 0
    {
        int N;

        X.Write2(X.Write3(x));
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer17_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger17, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    private const string MethodCallSourceCodeInteger18 = @"
using System;

class Program_Verifier_Integer18_1
{
    int Z;

    public void WriteY()
    {
    }
}
// Invariant: Z == 0
// Invariant: Z != 0

class Program_Verifier_Integer18_2
{
    Program_Verifier_Integer18_1 X = new();

    public void Write()
    {
        X.WriteY();
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer18_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger18, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private const string MethodCallSourceCodeInteger19 = @"
using System;

class Program_Verifier_Integer19_1
{
    public void Other()
    {
    }

    public static void Write(int n)
    // Require: n > 0
    {
    }
}

class Program_Verifier_Integer19_2
{
    public void Write1(int n)
    // Require: n > 0
    {
        Program_Verifier_Integer19_1.Write(n);
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer19_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MethodCallSourceCodeInteger19, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }
}
