namespace MiscStatements.Test;

using System;
using ModelAnalyzer;
using NUnit.Framework;
using Verification.Test;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string MiscStatementSourceCode1 = @"
using System;

class Program_Verifier_MiscStatement1
{
    int X;
    public int Y { get; set; }

    public void Write()
    {
        X = X + 1;

        if (X == 1)
        {
            X = X * 1;

            if (X == 1)
            {
                X = X - 1;
            }
            else
                X = 1;
        }
        else
            X = 1;
    }
}
// Invariant: X == 0
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string MiscStatementSourceCode2 = @"
using System;

class Program_Verifier_MiscStatement2
{
    public int X { get; set; }

    public void Write1()
    {
        X = X + 1;

        if (X == 1)
        {
            X = X * 1;

            if (X == 1)
            {
            }
            else
                X = 2;
        }
        else
            X = 2;
    }

    void Write2()
    {
        X = X + 2;

        if (X == 2)
        {
            X = X * 2;

            if (X == 4)
            {
                X = X - 3;
            }
            else
                X = 2;
        }
        else
            X = 2;
    }
}
// Invariant: X == 0 || X == 1
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement2_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Statement2_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode2, maxDepth: 2, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private const string MiscStatementSourceCode3 = @"
using System;

class Program_Verifier_MiscStatement3
{
    int X;

    public void Write()
    {
        X = X + 1;
    }
}
// Invariant: X == 0 || X == 1
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement3_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Statement3_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode3, maxDepth: 2, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Statement3_ErrorAgain()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode3, maxDepth: 3, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private const string MiscStatementSourceCode4 = @"
using System;

class Program_Verifier_MiscStatement4
{
    int X;

    public void Write()
    {
        int Y;

        Y = X + 1;
        X = Y;
    }
}
// Invariant: X == 0 || X == 1
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement4_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Statement4_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode4, maxDepth: 2, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Statement4_ErrorAgain()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode4, maxDepth: 3, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    private const string MiscStatementSourceCode5 = @"
using System;

class Program_Verifier_MiscStatement5
{
    public int Write(int x)
    // Require: x == 0
    {
        int X;
        int Result;

        Result = x;
        return Result;
    }
    // Ensure: Result == 0
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement5_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode5, maxDepth: 2, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string MiscStatementSourceCode6 = @"
using System;

class Program_Verifier_MiscStatement6
{
    int X;

    public void Write(int x)
    {
        if (Write2(x) == 1)
            X = x;
    }

    int Write2(int x)
    // Require: x == 0
    {
        return x;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement6_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    private const string MiscStatementSourceCode7 = @"
using System;

class Program_Verifier_MiscStatement7
{
    int Write2(int x)
    // Require: x == 1
    {
        return x;
    }
}
// Invariant: Write2(0) == 1
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement7_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode7, maxDepth: 0, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    private const string MiscStatementSourceCode8 = @"
using System;

class Program_Verifier_MiscStatement8
{
    int X;

    public void Write(int x)
    // Require: Write2(0) == 0
    {
        X = x;
    }

    int Write2(int x)
    {
        return x;
    }
    // Ensure: Result == 1
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement8_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode8, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    private const string MiscStatementSourceCode9 = @"
using System;

class Program_Verifier_MiscStatement9
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
    // Ensure: Write2(0) == 0

    int Write2(int x)
    {
        return x;
    }
    // Ensure: Result == 1
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement9_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode9, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    private const string MiscStatementSourceCode10 = @"
using System;

class Program_Verifier_MiscStatement10
{
    int X;

    public void Write(int x, int y)
    // Require: y > 0
    {
        X = x % y;
        X = (x + 1) % (y + 1);
        X = (x % y) + (x % y);
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement10_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode10, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string MiscStatementSourceCode11 = @"
using System;

class Program_Verifier_MiscStatement11
{
    int X;

    public void Write(int x, int y)
    {
        X = x / y;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement11_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode11, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.AssumeError));
    }

    private const string MiscStatementSourceCode12 = @"
using System;

class Program_Verifier_MiscStatement12
{
    int X;

    public void Write(int x, int y)
    {
        X = x % y;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement12_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode12, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.AssumeError));
    }

    private const string MiscStatementSourceCode13 = @"
using System;

class Program_Verifier_MiscStatement13
{
    int X;
    int Y;

    public void Write(int x, int y)
    // Require: x >= 0
    // Require: y >= 0
    {
        X = x;
        Y = y;
    }
}
// Invariant: X / Y >= 0
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement13_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode13, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.AssumeError));
    }

    private const string MiscStatementSourceCode14 = @"
using System;

class Program_Verifier_MiscStatement14
{
    public int Write(int x)
    // Require: Write2(x) == 1
    {
        return x;
    }
    // Ensure: Result == 1

    int Write2(int x)
    {
        return x;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement14_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode14, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string MiscStatementSourceCode15 = @"
using System;

class Program_Verifier_MiscStatement15
{
    public int Write(int x)
    // Require: x == 1
    {
        return x;
    }
    // Ensure: Write2(Result) == 1

    int Write2(int x)
    {
        return x;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement15_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode15, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string MiscStatementSourceCode16 = @"
using System;

class Program_Verifier_MiscStatement16
{
    public int Write(int x)
    // Require: Write2(x) == 1
    {
        return x;
    }
    // Ensure: Result != 1

    int Write2(int x)
    {
        return x;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement16_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode16, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    private const string MiscStatementSourceCode17 = @"
using System;

class Program_Verifier_MiscStatement17
{
    public int Write(int x)
    // Require: x != 1
    {
        return x;
    }
    // Ensure: Write2(Result) == 1

    int Write2(int x)
    {
        return x;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement17_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode17, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.False);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    private const string MiscStatementSourceCode18 = @"
using System;

class Program_Verifier_MiscStatement18
{
    public int Read(int x)
    {
        int Result;

        if (x == 0)
        {
            Result = Read2(x);
        }
        else
        {
            Result = Read3(x);
        }

        return Result;
    }

    int Read2(int y)
    {
        return 0;
    }

    int Read3(int z)
    {
        return 0;
    }
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Statement18_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(MiscStatementSourceCode18, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }
}
