namespace FunctionCall.Test;

using System;
using ModelAnalyzer;
using NUnit.Framework;
using Verification.Test;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string FunctionCallSourceCodeInteger1 = @"
using System;

class Program_Verifier_Integer1
{
    public int Write1()
    {
        return Write2();
    }

    public int Write2()
    {
    }
}
";

    private const string FunctionCallSourceCodeInteger2 = @"
using System;

class Program_Verifier_Integer2
{
    int X;

    public void Write1(int x)
    {
        X = Write2(x);
    }

    int Write2(int x)
    {
        return x;
    }
}
// Invariant: X == 0
";

    private const string FunctionCallSourceCodeInteger3 = @"
using System;

class Program_Verifier_Integer3
{
    int X;
    int Y;

    public void Write1(int y)
    {
        Y = Write2(y);
    }

    int Write2(int y)
    {
        return Write3(y);
    }

    int Write3(int y)
    {
        return y;
    }
}
// Invariant: Y == 0
";

    private const string FunctionCallSourceCodeInteger4 = @"
using System;

class Program_Verifier_Integer4
{
    int X;

    public void Write1(int x)
    {
        X = Write2(x);
    }

    int Write2(int x)
    // Require: x == 0
    {
        return x;
    }
}
// Invariant: X == 0
";

    private const string FunctionCallSourceCodeInteger5 = @"
using System;

class Program_Verifier_Integer5
{
    int X;

    public void Write1(int x)
    {
        X = Write2(0);
    }

    int Write2(int x)
    // Require: x == 0
    {
        return x;
    }
}
// Invariant: X == 0
";

    private const string FunctionCallSourceCodeInteger6 = @"
using System;

class Program_Verifier_Integer6
{
    public int X { get; set; }

    public void Write1(int x)
    {
        X = x;
        X = Write2(0);
    }
    // Ensure: X == 0

    int Write2(int x)
    {
        return x;
    }
}
// Invariant: X == 0
";

    private const string FunctionCallSourceCodeInteger7 = @"
using System;

class Program_Verifier_Integer7
{
    public int X { get; set; }

    public int Write1(int x)
    {
        X = Write2(1);
        return X;
    }
    // Ensure: X == 0

    int Write2(int x)
    {
        return x;
    }
}
";

    private const string FunctionCallSourceCodeInteger8 = @"
using System;

class Program_Verifier_Integer8
{
    public int X { get; set; }

    public int Write1(int x)
    {
        X = Write2(0);
        return X;
    }
    // Ensure: X == 0

    int Write2(int x)
    {
        return x;
    }
}
";

    private const string FunctionCallSourceCodeInteger9 = @"
using System;

class Program_Verifier_Integer9
{
    int X;

    public void Write1(int x)
    {
        X = Write2(x);
    }

    int Write2(int x)
    {
        return Write3(x);
    }

    int Write3(int x)
    // Require: x == 0
    {
        return x;
    }
}
// Invariant: X == 0
";

    private const string FunctionCallSourceCodeInteger10 = @"
using System;

class Program_Verifier_Integer10
{
    public int X { get; set; }

    public void Write1(int x)
    {
        X = Write2(x);
    }
    // Ensure: X == 1

    int Write2(int x)
    {
        return Write3(x);
    }
    // Ensure: Result == 0

    int Write3(int x)
    {
        return x;
    }
}
// Invariant: X == 0
";

    private const string FunctionCallSourceCodeInteger11 = @"
using System;

class Program_Verifier_Integer11
{
    public int Write1(int x)
    {
        int Result;

        Result = x;
        return Result;
    }
    // Ensure: Result == 0 && Result != 0
}
";

    private const string FunctionCallSourceCodeInteger12 = @"
using System;

class Program_Verifier_Integer12
{
    public int X { get; set; }

    public void Write1(int x)
    {
        X = Write2(x);
    }
    // Ensure: X == 0 && X != 0

    int Write2(int x)
    {
        return x;
    }
}
";

    private const string FunctionCallSourceCodeInteger13 = @"
using System;

class Program_Verifier_Integer13
{
    int X;

    public void Write1(int x)
    // Require: x == 0
    {
        X = Write2(Write3(x));
    }

    int Write2(int x)
    // Require: x == 0
    {
        return Write3(x);
    }

    int Write3(int x)
    // Require: x == 1
    {
        return x;
    }
}
";

    private const string FunctionCallSourceCodeInteger14 = @"
using System;

class Program_Verifier_Integer14
{
    int X;

    public void Write1(int x)
    {
        X = x;
    }
    // Ensure: Write2(x) == 0

    int Write2(int x)
    {
        return 0;
    }
    // Ensure: Write3(x) == 0

    int Write3(int x)
    {
        return x;
    }
    // Ensure: Result == 1
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer2_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer3_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer4_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer5_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer6_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer7_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger7, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer8_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger8, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer9_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger9, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer10_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger10, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer11_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger11, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer12_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger12, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer13_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger13, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer14_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(FunctionCallSourceCodeInteger14, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }
}
