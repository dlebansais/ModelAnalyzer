namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<BadParameterAnalyzer>;

[TestFixture]
public class BadParameterUnitTests
{
    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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
