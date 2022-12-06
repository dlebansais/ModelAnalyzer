namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<BadRequireAnalyzer>;

[TestClass]
public class BadRequireUnitTests
{
    [TestMethod]
    public async Task ValidExpression_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadRequire_0
{
    int X;

    void Write(int x)
    // Require: x != 0
    {
        X = x;
    }
}
");
    }

    [TestMethod]
    public async Task RequireWithInvalidCode_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadRequire_1
{
    int X;

    void Write(int x)
    // Require: [|x $ 0|]
    {
        X = x;
    }
}
");
    }

    [TestMethod]
    public async Task RequireWithTwoStatements_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadRequire_2
{
    int X;

    void Write(int x)
    // Require: [|x > 0; break;|]
    {
        X = x;
    }
}
");
    }

    [TestMethod]
    public async Task RequireWithUnsupportedExpression_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadRequire_3
{
    int X;

    void Write(int x)
    // Require: [|typeof(x)|]
    {
        X = x;
    }
}
");
    }

    [TestMethod]
    public async Task RequireWithUnknownField_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadRequire_4
{
    int X;

    void Write(int x)
    // Require: [|Y == 0|]
    {
        X = x;
    }
}
");
    }

    [TestMethod]
    public async Task MissplacedRequires_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadRequire_5
{
    int X;
    // Require: [|X == 0|]

    int Read() => X;
    // Require: [|X == 0|]

    int Y;
    // Require: [|X == 0|]
}
");
    }
}
