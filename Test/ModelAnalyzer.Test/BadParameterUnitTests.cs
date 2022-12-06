namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<BadParameterAnalyzer>;

[TestClass]
public class BadParameterUnitTests
{
    [TestMethod]
    public async Task ParameterShouldNotHaveAttribute_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadParameter_0
{
    void Write([|[System.Runtime.InteropServices.In] int x|])
    {
    }
}
");
    }

    [TestMethod]
    public async Task ParameterShouldNotHaveModifier_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadParameter_1
{
    void Write([|ref int x|])
    {
    }
}
");
    }

    [TestMethod]
    public async Task ParameterPredefinedTypeIsNotSupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadParameter_2
{
    void Write([|string p|])
    {
    }
}
");
    }

    [TestMethod]
    public async Task ParameterTypeIsNotSupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadParameter_3
{
    void Write([|Program_BadParameter_3 p|])
    {
    }
}
");
    }
}
