namespace ModelAnalyzer.Verification.Test;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    void Write()
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

    void Write1()
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
}
