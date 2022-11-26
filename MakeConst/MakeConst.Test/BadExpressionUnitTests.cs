namespace DemoAnalyzer.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<BadExpressionAnalyzer>;

[TestClass]
public class BadExpressionUnitTests
{
    [TestMethod]
    public async Task ExpressionIsUnsupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int X;

    void Write(int x)
    {
        X = [|nameof(X).Length|];
    }
}
");
    }

    [TestMethod]
    public async Task InvalidBinaryExpression_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int X;

    void Write(int x)
    {
        X = x [|%|] x;
    }
}
");
    }
}
