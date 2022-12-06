﻿namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<BadEnsureAnalyzer>;

[TestClass]
public class BadEnsureUnitTests
{
    [TestMethod]
    public async Task ValidExpression_NoDiagnostic()
    {
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

    [TestMethod]
    public async Task ValidEnsureAfterExpressionBody_NoDiagnostic()
    {
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

    [TestMethod]
    public async Task EnsureWithInvalidCode_Diagnostic()
    {
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

    [TestMethod]
    public async Task EnsureWithTwoStatements_Diagnostic()
    {
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

    [TestMethod]
    public async Task EnsureWithUnsupportedExpression_Diagnostic()
    {
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

    [TestMethod]
    public async Task EnsureWithUnknownField_Diagnostic()
    {
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

    [TestMethod]
    public async Task MissplacedEnsures_Diagnostic()
    {
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
