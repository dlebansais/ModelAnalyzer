namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<BadInvariantAnalyzer>;

[TestFixture]
public class BadInvarianttUnitTests
{
    [Test]
    [Category("Analyzer")]
    public async Task ClassWithNoInvariant_NoDiagnostic()
    {
        await ClassModelManager.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadInvariant_0
{
    int X;

    int Read()
    {
        return X;
    }

    void Write(int x)
    {
        if (x >= 0)
            X = x;
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithInvariant_NoDiagnostic()
    {
        await ClassModelManager.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadInvariant_1
{
    int X;

    int Read()
    {
        return X;
    }

    void Write(int x)
    {
        if (x >= 0)
            X = x;
    }
}
// Invariant: X >= 0
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithErrorInInvariant_Diagnostic()
    {
        await ClassModelManager.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadInvariant_2
{
    int X;

    int Read()
    {
        return X;
    }

    void Write(int x)
    {
        if (x >= 0)
            X = x;
    }
}
// Invariant: [|X $ 0|]
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithTwoStatementsInInvariant_Diagnostic()
    {
        await ClassModelManager.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadInvariant_3
{
    int X;

    int Read()
    {
        return X;
    }

    void Write(int x)
    {
        if (x >= 0)
            X = x;
    }
}
// Invariant: [|X >= 0; break;|]
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithBadExpressionInInvariant_Diagnostic()
    {
        await ClassModelManager.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadInvariant_4
{
    int X;

    int Read()
    {
        return X;
    }

    void Write(int x)
    {
        if (x >= 0)
            X = x;
    }
}
// Invariant: [|typeof(X)|]
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithInvertedExpressionInInvariant_NoDiagnostic()
    {
        await ClassModelManager.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadInvariant_5
{
    int X;

    int Read()
    {
        return X;
    }

    void Write(int x)
    {
        if (x >= 0)
            X = x;
    }
}
// Invariant: 0 <= X
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithBadOperatorInInvariant_Diagnostic()
    {
        await ClassModelManager.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadInvariant_6
{
    int X;

    int Read()
    {
        return X;
    }

    void Write(int x)
    {
        if (x >= 0)
            X = x;
    }
}
// Invariant: [|X % 0|]
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithBadConstantInInvariant_Diagnostic()
    {
        await ClassModelManager.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadInvariant_7
{
    int X;

    int Read()
    {
        return X;
    }

    void Write(int x)
    {
        if (x >= 0)
            X = x;
    }
}
// Invariant: [|X % X|]
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithUnknownFieldInInvariant_Diagnostic()
    {
        await ClassModelManager.SynchronizeWithVerifierAsync();

        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadInvariant_8
{
    int X;

    int Read()
    {
        return X;
    }

    void Write(int x)
    {
        if (x >= 0)
            X = x;
    }
}
// Invariant: [|Y >= 0|]
");
    }
}
