namespace DemoAnalyzer.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<BadInvariantAnalyzer>;

[TestClass]
public class BadInvarianttUnitTests
{
    [TestMethod]
    public async Task ClassWithNoInvariant_NoDiagnostic()
    {
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

    [TestMethod]
    public async Task ClassWithInvariant_NoDiagnostic()
    {
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

    [TestMethod]
    public async Task ClassWithErrorInInvariant_Diagnostic()
    {
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

    [TestMethod]
    public async Task ClassWithTwoStatementsInInvariant_Diagnostic()
    {
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

    [TestMethod]
    public async Task ClassWithBadExpressionInInvariant_Diagnostic()
    {
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

    [TestMethod]
    public async Task ClassWithInvertedExpressionInInvariant_NoDiagnostic()
    {
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

    [TestMethod]
    public async Task ClassWithBadOperatorInInvariant_Diagnostic()
    {
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

    [TestMethod]
    public async Task ClassWithBadConstantInInvariant_Diagnostic()
    {
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

    [TestMethod]
    public async Task ClassWithUnknownFieldInInvariant_Diagnostic()
    {
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
