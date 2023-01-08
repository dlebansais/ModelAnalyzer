namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

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

    private const string MiscStatementSourceCode14 = @"
using System;

class Program_Verifier_MiscStatement14_1
{
}

class Program_Verifier_MiscStatement14_2
{
    public Program_Verifier_MiscStatement14_1? X { get; set; } = null;

    public void Write()
    {
        X = new Program_Verifier_MiscStatement14_1();
    }
    // Ensure: X == null
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode1, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements2_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode2, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements2_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode2, maxDepth: 2, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements3_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode3, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements3_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode3, maxDepth: 2, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements3_ErrorAgain()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode3, maxDepth: 3, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements4_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode4, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements4_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode4, maxDepth: 2, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements4_ErrorAgain()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode4, maxDepth: 3, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements5_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode5, maxDepth: 2, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements6_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode6, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements7_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode7, maxDepth: 0, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.RequireError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements8_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode8, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements9_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode9, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements10_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode10, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements11_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode11, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.AssumeError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements12_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode12, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.AssumeError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements13_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode13, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.AssumeError));
    }

    /*
    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements14_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode14, maxDepth: 1, maxDuration: MaxDuration);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.AssumeError));
    }*/
}
