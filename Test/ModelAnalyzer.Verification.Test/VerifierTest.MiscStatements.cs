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
    int X;

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

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements1_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode1, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements2_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode2, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements2_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode2, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements3_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode3, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements3_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode3, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements3_ErrorAgain()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode3, maxDepth: 3);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements4_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode4, maxDepth: 1);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements4_Error()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode4, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements4_ErrorAgain()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode4, maxDepth: 3);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.InvariantError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_MiscStatements5_Success()
    {
        Verifier TestObject = CreateVerifierFromSourceCode(MiscStatementSourceCode5, maxDepth: 2);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }
}
