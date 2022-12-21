namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
// using VerifyCS = CSharpAnalyzerVerifier<BadExpressionAnalyzer>;
using VerifyCS = CSharpAnalyzerVerifier<InvalidElementAnalyzer>;

[TestFixture]
public class BadExpressionUnitTests
{
    [Test]
    [Category("Analyzer")]
    public async Task ExpressionIsUnsupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadExpression_0
{
    int X;

    void Write(int x)
    {
        X = [|nameof(X).Length|]MA0002;
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task InvalidBinaryExpression_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadExpression_1
{
    int X;

    void Write(int x)
    {
        X = x [|%|]MA0002 x;
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidOperator_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadExpression_2
{
    int X;

    void Write(int x)
    {
        X = (x + x) - ((x * x) / x);
    }
}
");
    }
}
