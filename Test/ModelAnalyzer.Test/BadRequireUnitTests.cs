namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<InvalidElementAnalyzer>;

[TestFixture]
public class BadRequireUnitTests
{
    [Test]
    [Category("Analyzer")]
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

    [Test]
    [Category("Analyzer")]
    public async Task RequireWithInvalidCode_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadRequire_1
{
    int X;

    void Write(int x)
    // Require: [|x $ 0|]MA0005
    {
        X = x;
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task RequireWithTwoStatements_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadRequire_2
{
    int X;

    void Write(int x)
    // Require: [|x > 0; break;|]MA0005
    {
        X = x;
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task RequireWithUnsupportedExpression_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadRequire_3
{
    int X;

    void Write(int x)
    // Require: [|typeof(x)|]MA0005
    {
        X = x;
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task RequireWithUnknownField_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadRequire_4
{
    int X;

    void Write(int x)
    // Require: [|Y == 0|]MA0002
    {
        X = x;
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task MissplacedRequires_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadRequire_5
{
    int X;
    // Require: [|X == 0|]MA0005

    int Read() => X;
    // Require: [|X == 0|]MA0005

    int Y;
    // Require: [|X == 0|]MA0005
}
");
    }
}
