namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<BadEnsureAnalyzer>;

[TestFixture]
public class BadEnsureUnitTests
{
    [Test]
    [Category("Analyzer")]
    public async Task ValidExpression_NoDiagnostic()
    {
        await VerificationSynchronization.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadEnsure_0
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Ensure: X == x
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidEnsureAfterExpressionBody_NoDiagnostic()
    {
        await VerificationSynchronization.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadEnsure_1
{
    int X;

    int Read() => X;
    // Ensure: X == 0
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task EnsureWithInvalidCode_Diagnostic()
    {
        await VerificationSynchronization.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadEnsure_2
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Ensure: [|x $ 0|]
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task EnsureWithTwoStatements_Diagnostic()
    {
        await VerificationSynchronization.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadEnsure_3
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Ensure: [|x > 0; break;|]
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task EnsureWithUnsupportedExpression_Diagnostic()
    {
        await VerificationSynchronization.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadEnsure_4
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Ensure: [|typeof(x)|]
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task EnsureWithUnknownField_Diagnostic()
    {
        await VerificationSynchronization.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadEnsure_5
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Ensure: [|Y == 0|]
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task MissplacedEnsures_Diagnostic()
    {
        await VerificationSynchronization.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadEnsure_6
{
    int X;
    // Ensure: [|X == 0|]

    string Read() => string.Empty;
    // Ensure: [|X == 0|]

    int Y;
    // Ensure: [|X == 0|]
}
");
    }
}
